```
BUILD_ID=<Next.JS build id> \
RELEASE_PAGE_URL=/find-statistics/<publication-slug>/<release-slug> \
EXCLUDE_DATA_REQUESTS=false \
PRINT_URLS=true \
wrk2 -t5 -c20 -R250 -d30s -s release_page_redesign.lua https://<environment-url>
```