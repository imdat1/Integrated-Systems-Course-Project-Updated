This is just an updated version of this project: https://github.com/imdat1/Integrated-Systems-Course-Project

This project fully runs on .NET Core 8 and doesn't use any Python Flask app.

You can find the functions that changed the Python Flask app in the "Service". Specifically, these services are new:
- CheckDBConnection.cs
- DatabaseSchemaRetriever.cs
- HuggingFaceQuerryRunner.cs

The code that used to run with the Python Flask app can be found commented in the Database, Question, and Admin controllers so you can have a clearer idea of how the whole project changed overall.