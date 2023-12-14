**How to install Playwright and set up VS Code?**

# Here are the prerequisites to setting up VS Code:

* nodejs : Download and Install Node JS from
https://nodejs.org/en/download/

* VS Code Editor: Download and Install VS code from 
https://code.visualstudio.com/download
* Playwright VS Code plugin

# Installation

Step 1: Navigate to 'Playwright-tests' folder
Step 2: Ensure the NodeJS version is above 14 from the command prompt.
        node -v
Step 3:Launch the VS code
Step 4:Navigate to the EXTENSIONS section and type "Playwright". There are multiple options available. Select the "Playwright Test for VS Code by Microsoft option".
Step 6: Click Install.
Step 7: Press CTRL + SHIFT + P to open the command panel and type "install Playwright".

Step 8: Enable the Chromium, Firefox, and WebKit checkboxes, as Playwright supports all browser engines.

Step 9: Click Okay.

**Alternatively you can also install the playwright using npm/yarn/pnpm.**
npm :npm init playwright@latest
yarn:yarn create playwright
pnpm:pnpm create playwright

Please visit the following URL for more information
https://playwright.dev/docs/intro#installing-playwright

How to run the playwright test?

Running the end-to-end test: 
  npx playwright test

Running the test in single file:
  npx playwright test example
