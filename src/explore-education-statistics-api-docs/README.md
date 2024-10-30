# Explore education statistics API documentation

This repository is used to generate the documentation website for the Explore education statistics API.
It is based on the GOV.UK [Technical Documentation Template](https://tdt-documentation.london.cloudapps.digital/)
for building 

## Pre-requisites

The following pre-requisite dependencies are required to get started:

- [Node.js](https://nodejs.org/en/) v20+ (can be installed with [nvm](https://github.com/nvm-sh/nvm) or [fnm](https://github.com/Schniz/fnm))
- [Ruby](https://www.ruby-lang.org/en/) v3.3.5 (can be installed with [rbenv](https://github.com/rbenv/rbenv) or [rvm](https://rvm.io/))

As always, it's advisable to install any versions using a version manager to make it easier to upgrade 
and keep aligned with the project.

### Ubuntu

If you are using Ubuntu, you may need to install the following dependencies before you can install
Ruby and its required gems:

```shell
sudo apt install libssl-dev libyaml-dev
```

## Getting started

Once the pre-requisites have been installed, follow these steps:

1. Install the project's Ruby dependencies:

    ```shell
    bundle install
    ```
   
2. Start the development server:

    ```shell
    bundle exec middleman
    ```

    This will start the Middleman development server on [https://localhost:4567](https://localhost:4567).

3. Optional. To automatically refresh the browser upon code changes, install the [LiveReload browser extension](https://chrome.google.com/webstore/detail/livereload/jnihajbhpnppcggbcgedagnkighmdlei?hl=en).

For further guidance on how to develop this documentation, please visit the [Technical Documentation Template](https://tdt-documentation.london.cloudapps.digital/) website.
