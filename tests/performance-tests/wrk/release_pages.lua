require('utils')

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

local page_being_processed
local page_request_counter = 0

math.randomseed(os.time())

-- This test randomly selects a Release page (i.e. Home, Explore, Methodology, Help), and then runs through each of
-- its associated main and prefetch requests in turn.  When all requests for a page have been completed it will choose
-- a new page and repeat the process.
function request()

    -- If no page is currently selected for processing, select a new one to process at random, based on their weighted
    -- probabilities.
    if is_new_page_required(page_being_processed, page_request_counter + 1) then
        page_being_processed = get_random_page()
        page_request_counter = 1
    else
        page_request_counter = page_request_counter + 1
    end

    local next_request = page_being_processed.requests[page_request_counter]

    if print_urls then
        print('Processing request ', page_request_counter, ' of page ', page_being_processed.page_name, ': ', next_request.url)
    end

    if next_request.prefetch then
        return wrk.format("GET", next_request.url, next_prefetch_headers)
    else
        return wrk.format("GET", next_request.url, default_headers)
    end
end

function response(status, _, _)
    if print_responses then
        print('Got response code ', status)
    end
end

function is_new_page_required(current_page, next_request_number)

    -- If no page is currently being processed, we need a new page selection.
    if current_page == nil then
        return true
    end

    -- If we've reached the final request for the current page being processed, we need a new page selection.
    if next_request_number > #current_page.requests then
        return true
    end

    -- If the next request to process is a prefetch and we're excluding prefetch requests, we need a new page
    -- selection.
    if current_page.requests[next_request_number].prefetch and exclude_prefetch_requests then
        return true
    end

    -- Otherwise, continue to send requests from the existing page being processed.
    return false
end

function get_random_page()
    local random = math.random(running_total_weighting - 1)
    local random_page = table.filter_array(page_weightings, function(page_weighting)
        return random >= page_weighting.min and random < page_weighting.max
    end)[1]
    if print_urls then
        print('Processing page ', random_page.page_name)
    end
    return random_page
end