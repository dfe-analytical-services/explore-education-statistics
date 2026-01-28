local utils = require('utils')

local build_id = os.getenv("BUILD_ID")
local publication_slug = os.getenv("PUBLICATION_SLUG")
local release_slug = os.getenv("RELEASE_SLUG")
local exclude_prefetch_requests = os.getenv("EXCLUDE_PREFETCH_REQUESTS") == "true"
local print_urls = os.getenv("PRINT_URLS") == "true"
local print_responses = os.getenv("PRINT_RESPONSES") == "true"

local user = os.getenv("BASIC_AUTH_USERNAME")
local pass = os.getenv("BASIC_AUTH_PASSWORD")

local default_headers = {}
if user and pass then
    default_headers["Authorization"] = "Basic " .. base64_encode(user .. ":" .. pass)
end

local next_prefetch_headers = {
    ["x-nextjs-data"] = "1",
    ["x-middleware-prefetch"] = "1",
    ["purpose"] = "prefetch",
}

for k, v in pairs(default_headers) do
    next_prefetch_headers[k] = v
end

local release_page_url = '/find-statistics/' .. publication_slug .. '/' .. release_slug

local page_requests = {
    {
        page_name = 'Main release content page',
        requests = {
            -- This is an HTML request.
            {
                url = release_page_url .. "?redesign=true",
                prefetch = false,
            },
            -- This is a prefetch request.
            {
                url = "/_next/data/" .. build_id .. "/find-statistics.json",
                prefetch = true,
            },
        },
        weighting = 70
    },
    {
        page_name = 'Explore page',
        requests = {
            -- This is a JSON request.
            {
                url = "/_next/data/" .. build_id
                        .. release_page_url
                        .. "/explore.json?publication="
                        .. publication_slug
                        .. "&release="
                        .. release_slug
                        .. "&tab=explore",
                prefetch = false,
            },
            -- This is a prefetch request.
            {
                url = "/_next/data/" .. build_id
                        .. release_page_url
                        .. "/explore.json?publication="
                        .. publication_slug
                        .. "&release="
                        .. release_slug
                        .. "&tab=explore",
                prefetch = true,
            },
        },
        weighting = 10
    },
    {
        page_name = 'Methodology page',
        requests = {
            -- This is a JSON request.
            {
                url = "/_next/data/" .. build_id
                        .. release_page_url
                        .. "/methodology.json?publication="
                        .. publication_slug
                        .. "&release="
                        .. release_slug
                        .. "&tab=methodology",
                prefetch = false,
            },
            -- This is a prefetch request.
            {
                url = "/_next/data/" .. build_id
                        .. release_page_url
                        .. "/methodology.json?publication="
                        .. publication_slug
                        .. "&release="
                        .. release_slug
                        .. "&tab=methodology",
                prefetch = true,
            },
        },
        weighting = 10
    },
    {
        page_name = 'Help page',
        requests = {
            -- This is a JSON request.
            {
                url = "/_next/data/" .. build_id
                        .. release_page_url
                        .. "/help.json?publication="
                        .. publication_slug
                        .. "&release="
                        .. release_slug
                        .. "&tab=help",
                prefetch = false,
            },
            -- This is a prefetch request.
            {
                url = "/_next/data/" .. build_id
                        .. release_page_url
                        .. "/help.json?publication="
                        .. publication_slug
                        .. "&release="
                        .. release_slug
                        .. "&tab=help",
                prefetch = true,
            },
        },
        weighting = 10
    }
}

local running_total_weighting = 0
local page_weightings = {}
for _, page in ipairs(page_requests) do
    table.insert(page_weightings, {
        page_name = page.page_name,
        min = running_total_weighting,
        max = running_total_weighting + page.weighting,
        requests = page.requests
    })
    running_total_weighting = running_total_weighting + page.weighting
end

local page_being_processed = nil
local page_request_counter = 0

math.randomseed(os.time())

-- This test randomly selects a Release page (i.e. Home, Explore, Methodology, Help), and then runs through each of
-- its associated main and prefetch requests in turn.  When all requests for a page have been completed it will choose
-- a new page and repeat the process.
function request()

    page_request_counter = page_request_counter + 1

    local next_request

    -- If we're currently processing a page's requests and we've not reached the end of its list of requests yet,
    -- select the next one.
    if page_being_processed ~= nil and page_request_counter <= #page_being_processed.requests then
        next_request = page_being_processed.requests[page_request_counter]
    else
        -- If we've reached the final request for the current page being processed, set the current page to nil so that
        -- a new one will be chosen for the next request.
        page_being_processed = nil
    end

    -- If the next request to be processed is the start of our prefetch requests and we're excluding prefetch requests,
    -- deselect the current page being processed so that a new one will be chosen for the next request.
    if next_request ~= nil and next_request.prefetch and exclude_prefetch_requests then
        page_being_processed = nil
    end

    -- If no page is currently selected for being processed, select a new one and proceed to run through its requests
    -- one by one.
    if page_being_processed == nil then
        random = math.random(running_total_weighting - 1)
        page_being_processed = table.filter_array(page_weightings, function(page_weighting)
            return random >= page_weighting.min and random < page_weighting.max
        end)[1]
        page_request_counter = 1
        next_request = page_being_processed.requests[page_request_counter]
        if print_urls then
            print('Processing page ', page_being_processed.page_name)
        end
    end

    if print_urls then
        print('Processing request ', page_request_counter, ' of page ', page_being_processed.page_name, ': ', next_request.url)
    end

    if next_request.prefetch then
        return wrk.format("GET", next_request.url, next_prefetch_headers)
    else
        return wrk.format("GET", next_request.url, default_headers)
    end
end

function response(status, headers, body)
    if print_responses then
        print('Got response code ', status)
    end
end