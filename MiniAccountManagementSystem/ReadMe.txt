The steps needed to Run the project 

step 1 :- Change the Server in appsettings.json file to to your MSSQL Server.

step 2:- Open MiniAMS_DB.sql file and run it.

step 3:- run update-database in Package Maneger Console.

step 4:- Run "MiniAccountManagementSystem" project and Register a user and Logout.

step 5:- Open MSSQL Server > Open Database > Find MiniAMS_DB > Open Table

step 6:- Open dbo.AspNetUsers > Open dbo.AspNetRoles  >  Open dbo.AspNetUserRoles

step 7:-  Copy dbo.AspNetUsers (Id) and paste in dbo.AspNetUserRoles (UserId).

step 8:-  Copy dbo.AspNetRoles (Id(Admin))  and paste in dbo.AspNetUserRoles (RoleId).

step 9:- Run "MiniAccountManagementSystem" project and Login using the Register user to access admin page.






