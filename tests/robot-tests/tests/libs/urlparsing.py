from urllib.parse import urlparse, urlunparse

def normalize_url(url):

    # Parse the URL into components
    parsed_url = urlparse(url)
    print(f"Parsed URL: {parsed_url}")

    # Remove the username and password by setting the netloc to just the hostname
    netloc = parsed_url.hostname
    if parsed_url.port:
        netloc += f":{parsed_url.port}"
    print(f"Modified Netloc: {netloc}")

    # Construct the normalized URL
    normalized_url = urlunparse((parsed_url.scheme, netloc, parsed_url.path, parsed_url.params, parsed_url.query, parsed_url.fragment))
    print(f"Normalized URL: {normalized_url}")

    return normalized_url
