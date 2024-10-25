using System.Net;
using System.Security.Claims;
using System.Text;
using Domain.Domain;
using Domain.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Interface;

namespace AppWebDB.Controllers
{
    public class DatabaseController : Controller
    {
        private readonly IDatabaseService _databaseService;
        //private readonly HttpClient _flaskClient;
        private readonly IDatabaseSchemaRetriever _databaseSchemaRetriever;
        private readonly ICheckDBConnection _checkDBConnection;

        public DatabaseController(IDatabaseService databaseService, IDatabaseSchemaRetriever databaseSchemaRetriever, ICheckDBConnection checkDBConnection)
        {
            _databaseService = databaseService;
            _databaseSchemaRetriever = databaseSchemaRetriever;
            _checkDBConnection = checkDBConnection;
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            List<Database> db = _databaseService.GetDatabasesForUser(userId);
            return View(db);
        }

        public IActionResult Create() {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync([Bind("Id,Name,Username,Password,Host,DatabaseName")] Database db)
        {
            if (ModelState.IsValid)
            {
                //Check DB credentials whether they are true
                var cn = await _checkDBConnection.CheckDatabaseCredentialsAsync(db.Host, db.DatabaseName, db.Username, db.Password);

                if (cn == true)
                {
                    // Database connection successful, proceed with creating the database
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    db.Id = Guid.NewGuid();
                    _databaseService.CreateNewDatabase(db, userId);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Database connection failed, return error message
                    ModelState.AddModelError(string.Empty, "Failed to connect to the database. Please check and re-enter your credentials.");
                    return View(db);
                }

                //BELOW COMMENTED CODE FOR WORKING WITH PYTHON FLASK APP
                // Send a POST request to the Flask app to check database connection
                //var formData = new Dictionary<string, string>
                //{
                //    { "dbUsername", db.Username },
                //    { "dbPass", db.Password },
                //    { "dbHost", db.Host },
                //    { "dbName", db.DatabaseName }
                //};

                //var connectResponse = await _flaskClient.PostAsync("http://localhost:5000/connect", new FormUrlEncodedContent(formData));

                //if (connectResponse.IsSuccessStatusCode)
                //{
                //    // Database connection successful, proceed with creating the database
                //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                //    db.Id = Guid.NewGuid();
                //    _databaseService.CreateNewDatabase(db, userId);
                //    return RedirectToAction(nameof(Index));
                //}
                //else
                //{
                //    // Database connection failed, return error message
                //    ModelState.AddModelError(string.Empty, "Failed to connect to the database. Please check and re-enter your credentials.");
                //    return View(db);
                //}
            }
            return View(db);
        }

        public IActionResult Edit(Guid? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var db = _databaseService.GetDetailsForDatabase(id);
            if(db == null)
            {
                return NotFound();
            }
            return View(db);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsync(Guid id, [Bind("Id,Name,Username,Password,Host,DatabaseName")] Database db)
        {
            if (id != db.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

                    //Check DB credentials whether they are true
                    var cn = await _checkDBConnection.CheckDatabaseCredentialsAsync(db.Host, db.DatabaseName, db.Username, db.Password);
                    if(cn==true)
                    {
                        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        _databaseService.UpdateExistingDatabase(db, userId);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to connect to the database. Please check and re-enter your credentials.");
                        return View(db);
                    }

                    //BELOW COMMENTED CODE FOR WORKING WITH PYTHON FLASK APP
                    //var formData = new Dictionary<string, string>
                    //{
                    //    { "dbUsername", db.Username },
                    //    { "dbPass", db.Password },
                    //    { "dbHost", db.Host },
                    //    { "dbName", db.DatabaseName }
                    //};

                    //var connectResponse = await _flaskClient.PostAsync("http://localhost:5000/connect", new FormUrlEncodedContent(formData));

                    //if (connectResponse.IsSuccessStatusCode)
                    //{
                    //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    //    _databaseService.UpdateExistingDatabase(db, userId);
                    //}
                    //else
                    //{
                    //    // Database connection failed, return error message
                    //    ModelState.AddModelError(string.Empty, "Failed to connect to the database. Please check and re-enter your credentials.");
                    //    return View(db);
                    //}
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(db);
        }

        public IActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var db = _databaseService.GetDetailsForDatabase(id);
            if (db == null)
            {
                return NotFound();
            }

            return View(db);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _databaseService.DeleteDatabase(id, userId);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult SeeQuestions(Guid id)
        {
            var db = _databaseService.GetDetailsForDatabase(id);
            return RedirectToAction("Index","Question", new {id = id});
        }
    }
}
