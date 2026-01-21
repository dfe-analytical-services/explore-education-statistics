# EES wrk2 performance tests

## Release home page (and associated subpages)

This test script targets the public release Home page and its associated Explore, Methodology and Help pages.

It combines main page load requests with associated Next.JS `/_next/data` prefetch requests.

By default, it spreads traffic out between the 4 pages based on `weightings`. See the `weighting` field in the
`page_requests` table to see the allocation in [release_home_page.lua](release_home_page.lua).

```
BUILD_ID=<Next.JS build id> \
THEME_ID=<theme id of release> \
PUBLICATION_ID=<publication id of release> \
RELEASE_VERSION_ID=<release version id> \
PUBLICATION_SLUG=<publication slug> \
RELEASE_SLUG=<release slug> \
EXCLUDE_DATA_REQUESTS=false \
PRINT_URLS=true \
wrk2 -t5 -c20 -R250 -d30s -s release_home_page.lua https://<environment url>
```