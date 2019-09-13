#Postman and Newman tests

##Set up Postman with the API endpoints

1. Install Postman.
1. Choose "Import" -> "Import Folder", and import the "tests/newman/tests" folder.
1. Choose "Manage environments" -> "Import" and import each of the files in "tests/newman/environments".
1. Choose "Settings" and change the Workspace Folder to "<codebase folder>/tests/newman".

##Calling endpoints

###Prerequisites

1. If calling endpoints against the Admin application, you will need to get an Azure AD cookie for the environment 
that you will be targeting. 
    1. Visit the Admin application for the given environment.
    1. Click Sign in.
    1. Sign in on login.microsoftonline.com.
    1. You will be redirected back to the Admin application.  Copy the value of the ".AspNetCore.AzureADCookie" cookie
    that is now available in your browser.
    1. In Postman, click Manage Environments, and click on the environment that you are testing on.
    1. Paste the cookie value into the "azure_ad_cookie" variable.
1. If running any of the "Release - Data file uploads" or "Release - small file uploads" tests, you will need to 
supply a value for the environment variable "file_upload_validation_release_id".
    1. Visit the Admin application for the given environment.
    1. Find or create a Release against which to run the file upload tests against.
    1. Copy the Release ID from the URL.
    1. In Postman, click Manage Environments, and click on the environment that you are testing on.
    1. Paste the Release ID value into the "file_upload_validation_release_id" variable.
    
###Making calls

1. From the Environments dropdown, choose the environment you will be making calls against.
1. Open the desired endpoint collection (e.g. "DfE Admin API"), choose an endpoint to call, and hit "Send".
