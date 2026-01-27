# EES wrk2 performance tests

## Public Release page (and associated subpages)

This test script targets the public Release page and its associated Explore, Methodology and Help pages.

It combines main page load requests with associated Next.JS `/_next/data` prefetch requests.

By default, it spreads traffic out between the 4 pages based on `weightings`. See the `weighting` field in the
`page_requests` table to see the allocation in [release_pages.lua](release_pages.lua).

Example usage:

```
BUILD_ID=<Next.JS build ID> \
PUBLICATION_SLUG=<publication slug> \
RELEASE_SLUG=<release slug> \
EXCLUDE_DATA_REQUESTS=false \
PRINT_URLS=true \
PRINT_RESPONSES=true \
BASIC_AUTH_USERNAME=<username> \
BASIC_AUTH_PASSWORD=<password> \
wrk2 -t5 -c20 -R250 -d30s -s release_pages.lua https://<environment url>
```

The Next.JS build ID can be obtained for an environment by visiting a public page, viewing the source, and
searching for "buildId".