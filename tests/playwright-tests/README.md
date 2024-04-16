# How to install Playwright and set up VS Code?

## Here are the prerequisites to setting up VS Code:

* Download and install [NodeJS](https://nodejs.org/en/download/)

* Download and install [VSCode](https://code.visualstudio.com/download)
* Playwright VSCode plugin
* ES-lint extension
* Playwright test runner


## Installation (VSCode)

1. Navigate to 'Playwright-tests' folder
1. Ensure the NodeJS version is above 14 from the command prompt.
```bash
node -v
```
        
3. Launch the VSCode
1. Navigate to the EXTENSIONS section (*Ctrl + Shift + X*) and type "Playwright". There are multiple options available. Select the "Playwright Test for VSCode by Microsoft option".
1. Click Install.
1. Press *Ctrl + Shift + P* to open the command panel and type *"install Playwright"*.
1. Enable the Chromium, Firefox, and WebKit checkboxes, as Playwright supports all browser engines.
1. Click Okay.

**Alternatively you can also install the playwright using following command.**
```bash
pnpm create playwright
```

1. Choose between TypeScript or JavaScript (default is TypeScript)
1. Name of your Tests folder (default is `tests` or `e2e` if you already have a tests folder in your project)
1. Add a GitHub Actions workflow to easily run tests on CI
1. Install Playwright browsers (default is true)

For more information refer to the documentation on [installing Playwright](https://playwright.dev/docs/intro#installing-playwright)

## Installation (PyCharm)

**Run in PyCharm terminal the following command**

```bash
pnpm create playwright
```

1. Choose between TypeScript or JavaScript (default is TypeScript)
1. Name of your Tests folder (default is `tests` or `e2e` if you already have a tests folder in your project)
1. Add a GitHub Actions workflow to easily run tests on CI
1. Install Playwright browsers (default is true)

## How to run the playwright test?

### Running the end-to-end test: 
```bash
npx playwright test
```

### Running the test in single file:
```bash
npx playwright test <spec-filename.ts>
```

### Running the test in a folder level
```bash
npx playwright test --project "projectname"
```

### Running the test in debug mode:
```bash
npx playwright test <spec-filename.ts>  --debug
```
