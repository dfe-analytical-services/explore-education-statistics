import getUrlOrigin, { UrlOrigin } from '@common/utils/url/getUrlOrigin';
import formatContentLink from '@common/utils/url/formatContentLink';
import buildRel from '@common/utils/url/buildRel';

interface ContentLinkProps {
  url: string;
  target?: '_blank';
  rel?: string;
  origin: UrlOrigin;
  text?: string;
}

export default function getContentLinkProps(options: {
  url: string;
  text?: string;
  rel?: string;
}): ContentLinkProps {
  const { url, text, rel } = options;
  const origin = getUrlOrigin(url);
  const formattedUrl = formatContentLink(url);

  switch (origin) {
    case 'public':
      return {
        url: formattedUrl,
        origin,
        text,
      };
    case 'admin':
    case 'external-trusted':
      return {
        url: formattedUrl,
        target: '_blank',
        rel: buildRel(['nofollow', 'noopener', 'noreferrer'], rel),
        origin,
        text: addNewTabWarning(text),
      };
    default:
      return {
        url: formattedUrl,
        target: '_blank',
        rel: buildRel(['noopener', 'noreferrer', 'nofollow', 'external'], rel),
        origin,
        text: addNewTabWarning(text),
      };
  }
}

export function addNewTabWarning(text?: string): string {
  const trimmedText = text?.trim();
  if (trimmedText === undefined || trimmedText === '') {
    return '(opens in a new tab)';
  }

  if (trimmedText.endsWith('(opens in a new tab)')) {
    return trimmedText;
  }

  return `${trimmedText} (opens in a new tab)`;
}
