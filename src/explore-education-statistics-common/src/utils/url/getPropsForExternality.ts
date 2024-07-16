import getExternality, { Externality } from '@common/utils/url/getExternality';
import formatContentLink from '@common/utils/url/formatContentLink';
import buildRel from '@common/utils/url/buildRel';

interface ExternalityProps {
  url: string;
  target?: '_blank';
  rel?: string;
  externality: Externality;
  text?: string;
}

export default function getPropsForExternality(options: {
  url: string;
  text?: string;
  rel?: string;
}): ExternalityProps {
  const { url, text, rel } = options;
  const externality = getExternality(url);
  const formattedUrl = formatContentLink(url);

  switch (externality) {
    case 'internal':
      return {
        url: formattedUrl,
        externality,
        text,
      };
    case 'external-admin':
    case 'external-trusted':
      return {
        url: formattedUrl,
        target: '_blank',
        rel: buildRel(['nofollow', 'noopener', 'noreferrer'], rel),
        externality,
        text: addNewTabWarning(text),
      };
    default:
      return {
        url: formattedUrl,
        target: '_blank',
        rel: buildRel(['noopener', 'noreferrer', 'nofollow', 'external'], rel),
        externality,
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
