local buildId = os.getenv("BUILD_ID")
local releasePageUrl = os.getenv("RELEASE_PAGE_URL")
local exclude_data_requests = os.getenv("EXCLUDE_DATA_REQUESTS") == "true"
local print_urls = os.getenv("PRINT_URLS") == "true"

page_requests = {
    {
        page_name = 'Main release content page',
        requests = {
            "/find-statistics/foundation-years-ad-hoc/2018-19?redesign=true",
            "/_next/data/" .. buildId .. releasePageUrl .. "/releases.json?publication=foundation-years-ad-hoc",
            "/_next/data/" .. buildId .. releasePageUrl .. ".json?publication=foundation-years-ad-hoc&release=2018-19",
            "/_next/data/" .. buildId .. releasePageUrl .. "/explore.json?publication=foundation-years-ad-hoc&release=2018-19&tab=explore",
            "/_next/data/" .. buildId .. releasePageUrl .. "/methodology.json?publication=foundation-years-ad-hoc&release=2018-19&tab=methodology",
            "/_next/data/" .. buildId .. releasePageUrl .. "/help.json?publication=foundation-years-ad-hoc&release=2018-19&tab=help",
            "/_next/data/" .. buildId .. "/find-statistics.json"
        },
        weighting = 100
    },
    {
        page_name = 'Explore page',
        requests = {
            "/_next/data/" .. buildId .. releasePageUrl .. "/explore.json?publication=foundation-years-ad-hoc&release=2018-19&tab=explore",
            "/_next/data/" .. buildId .. "/data-catalogue.json?themeId=2ca22e34-b87a-4281-a0eb-b80f4f8dd374&publicationId=24f63a6f-5a5a-4025-d8b5-08d88b0047f4&releaseVersionId=e642795f-22ea-4eb6-957b-08d88ad5b210",
            "/_next/data/" .. buildId .. releasePageUrl .. "/explore.json?publication=foundation-years-ad-hoc&release=2018-19&tab=explore",
        },
        weighting = 0
    },
    {
        page_name = 'Methodology page',
        requests = {
            "/_next/data/" .. buildId .. releasePageUrl .. "/methodology.json?publication=foundation-years-ad-hoc&release=2018-19&tab=methodology",
        },
        weighting = 0
    },
    {
        page_name = 'Help page',
        requests = {
            "/_next/data/" .. buildId .. releasePageUrl .. "/help.json?publication=foundation-years-ad-hoc&release=2018-19&tab=help"
        },
        weighting = 0
    }
}

running_total_weighting = 0
page_weightings = {}
for _, page in ipairs(page_requests) do
    table.insert(page_weightings, { 
        page_name = page.page_name,
        min = running_total_weighting,
        max = running_total_weighting + page.weighting,
        requests = page.requests
    })
    running_total_weighting = running_total_weighting + page.weighting
end

math.randomseed(os.time())

page_being_processed = nil
page_request_counter = nil

local nextPrefetchHeaders = {
    ["x-nextjs-data"] = "1",
    ["x-middleware-prefetch"] = "1",
    ["purpose"] = "prefetch"
}

request = function()
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

    if string.find(request_path, '/_next/data') then
        return wrk.format("GET", request_path, nextPrefetchHeaders)
    else
        return wrk.format("GET", request_path)
    end
end

table.filter_array = function(t, filterIter)
    local out = {}
    local filteredTableIndex = 1

    for _, v in pairs(t) do
        if filterIter(v) then
            out[filteredTableIndex] = v
            filteredTableIndex = filteredTableIndex + 1
        end
    end

    return out
end