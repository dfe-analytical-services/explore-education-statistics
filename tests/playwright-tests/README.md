**How to install Playwright and set up VS Code?**

# Here are the prerequisites to setting up VS Code:

* nodejs : Download and Install Node JS from
https://nodejs.org/en/download/

* VS Code Editor: Download and Install VS code from 
https://code.visualstudio.com/download
* Playwright VS Code plugin
* ES-lint extension
* Playwright test runner


# Installation

1.  Navigate to 'Playwright-tests' folder
1.  Ensure the NodeJS version is above 14 from the command prompt.

        ```bash 
           node -v
        ```
1.  Launch the VS code
1.  Navigate to the EXTENSIONS section and type "Playwright". There are multiple options available. Select the "Playwright Test for VS Code by Microsoft option".
1.  Click Install.
1.  Press CTRL + SHIFT + P to open the command panel and type "install Playwright".
1.  Enable the Chromium, Firefox, and WebKit checkboxes, as Playwright supports all browser engines.
1.  Click Okay.


**Alternatively you can also install the playwright using following command.**

    ```bash
       pnpm create playwright
    ```

1.  Choose between TypeScript or JavaScript (default is TypeScript)
1.  Name of your Tests folder (default is tests or e2e if you already have a tests folder in your project)
1.  Add a GitHub Actions workflow to easily run tests on CI
1.  Install Playwright browsers (default is true)

Please visit the following URL for more information
https://playwright.dev/docs/intro#installing-playwright

**How to run the playwright test**?

## Running the end-to-end test: 

 ```bash
  npx playwright test
 ```

## Running the test in single file:

 ```bash
  npx playwright test <spec-filename.ts>
 ```

## Running the test in debug mode:

  ```bash
  npx playwright test <spec-filename.ts>  --debug
  ```
