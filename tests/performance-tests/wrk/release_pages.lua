local utils = require('utils')

local build_id = os.getenv("BUILD_ID")
local publication_slug = os.getenv("PUBLICATION_SLUG")
local release_slug = os.getenv("RELEASE_SLUG")
local exclude_data_requests = os.getenv("EXCLUDE_DATA_REQUESTS") == "true"
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
            release_page_url .. "?redesign=true",
            -- This is a prefetch request.
            "/_next/data/" .. build_id .. "/find-statistics.json"
        },
        weighting = 70
    },
    {
        page_name = 'Explore page',
        requests = {
            -- This is a JSON request.
            "/_next/data/" .. build_id .. release_page_url .. "/explore.json?publication="  .. publication_slug .. "&release=" .. release_slug .. "&tab=explore",
            -- This is a prefetch request.
            "/_next/data/" .. build_id .. release_page_url .. "/explore.json?publication=" .. publication_slug .. "&release=" .. release_slug .. "&tab=explore",
        },
        weighting = 10
    },
    {
        page_name = 'Methodology page',
        requests = {
            -- This is a JSON request.
            "/_next/data/" .. build_id .. release_page_url .. "/methodology.json?publication=" .. publication_slug .. "&release=" .. release_slug .. "&tab=methodology",
            -- This is a prefetch request.
            "/_next/data/" .. build_id .. release_page_url .. "/methodology.json?publication=" .. publication_slug .. "&release=" .. release_slug .. "&tab=methodology",
        },
        weighting = 10
    },
    {
        page_name = 'Help page',
        requests = {
            -- This is a JSON request.
            "/_next/data/" .. build_id .. release_page_url .. "/help.json?publication=" .. publication_slug .. "&release=" .. release_slug .. "&tab=help",
            -- This is a prefetch request.
            "/_next/data/" .. build_id .. release_page_url .. "/help.json?publication=" .. publication_slug .. "&release=" .. release_slug .. "&tab=help",
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
local page_request_counter = nil

math.randomseed(os.time())

function request()
    if page_being_processed == nil then
        random = math.random(running_total_weighting - 1)
        page_being_processed = table.filter_array(page_weightings, function(page_weighting)
            return random >= page_weighting.min and random < page_weighting.max
        end)[1]
        page_request_counter = 1
        if print_urls then
            print('Processing page ', page_being_processed.page_name)
        end
    end
    
    local request_path = page_being_processed.requests[page_request_counter]

    if print_urls then
        print('Processing request ', page_request_counter, ' of page ', page_being_processed.page_name, ': ', request_path)
    end
    
    if (page_request_counter == #page_being_processed.requests) or exclude_data_requests then
        page_being_processed = nil
    else
        page_request_counter = page_request_counter + 1
    end

    if page_request_counter == 1 or string.find(request_path, '/_next/data') then
        return wrk.format("GET", request_path, next_prefetch_headers)
    else
        return wrk.format("GET", request_path, default_headers)
    end
end

function response(status, headers, body)
    if print_responses then
        print('Got response code ', status)
    end
end