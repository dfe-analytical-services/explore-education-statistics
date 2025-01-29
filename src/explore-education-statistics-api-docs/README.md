# Explore education statistics API documentation

This repository is used to generate the documentation website for the Explore education statistics API.
It is based on the GOV.UK [Technical Documentation Template](https://tdt-documentation.london.cloudapps.digital/)

## Pre-requisites

The following pre-requisite dependencies are required to get started:

- [Node.js](https://nodejs.org/en/) v20+ (can be installed with [nvm](https://github.com/nvm-sh/nvm) or [fnm](https://github.com/Schniz/fnm))
- [Ruby](https://www.ruby-lang.org/en/) v3.2+ (can be installed with [rbenv](https://github.com/rbenv/rbenv) or [rvm](https://rvm.io/))

As always, it's advisable to install any versions using a version manager to make it easier to upgrade 
and keep aligned with the project.

### Ubuntu

If you are using Ubuntu, you may need to install the following dependencies before you can install
Ruby and its required gems:

```shell
sudo apt install build-essential zlib1g-dev libssl-dev libyaml-dev
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

3. **Optional** - To automatically refresh the browser upon code changes, install the [LiveReload browser extension](https://chrome.google.com/webstore/detail/livereload/jnihajbhpnppcggbcgedagnkighmdlei?hl=en).

For further guidance on how to develop this documentation, please visit the [Technical Documentation Template](https://tdt-documentation.london.cloudapps.digital/) website.

## Configuration

The project is configured via the `config/tech_docs.yml` file, which should contain the base defaults.
Some parts of the project config can be changed on a per-environment basis using a `.env` file, 
system environment variables or both.

The environment variables permitted and the config options they affect can be found in `config.rb`.

### Using `.env` file

For convenience during local development, an `.env` file can be used to set environment variables. 
Simply create a `.env` file in the project root and add any environment variables you want to set.

Environment variables will only override default config options if they are present in `.env`.
Omit any variables you aren't interested in changing.

### Using system environment variables

Simply add environment variables to your task runner or CLI command e.g.

```shell
TECH_DOCS_HOST=https://some-other-site bundle exec middleman 
```

## Generating API reference docs

API reference documentation is generated at build time using any `openapi-v*.json` documents that 
are found in the `source` directory. For example, to create the reference docs for a v2 API, add
a `openapi-v2.json`.

Refer to `lib/api_reference_pages_extension.rb` for how we implement this.

## Notifications for expired pages

All pages are configured with expiry dates so that they can be re-reviewed. We have automated the
checking of the expiry dates using a Rake task.

Whilst the site is running, run the following:

```shell
bundle exec rake notify:expired
```

This will list all the pages that have expired.

To check pages that will expire in the next month, you can also run:

```shell
bundle exec rake notify:expires
```

### Automated GitHub workflow

A GitHub workflow has been configured to run the `notify:expired` task on a scheduled cron (see 
`.github/workflows/notify-expired-api-docs.yml`). This has been set up to send notifications to
the Slack channel configured in `config/tech_docs.yml`.
