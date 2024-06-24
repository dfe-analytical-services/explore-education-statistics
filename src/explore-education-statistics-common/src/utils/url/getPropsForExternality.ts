import getExternality, { Externality } from '@common/utils/url/getExternality';
import formatContentLink from '@common/utils/url/formatContentLink';
import buildRel from '@common/utils/url/buildRel';

interface ExternalityProps {
  url: string;
  target: '_blank' | undefined;
  rel: string | undefined;
  externality: Externality;
  text?: string;
}

export default function getPropsForExternality(
  url: string | URL,
  text?: string,
  rel?: string,
): ExternalityProps {
  const externality = getExternality(url);
  const formattedUrl = formatContentLink(url, externality);

  switch (externality) {
    case 'internal':
      return {
        url: formattedUrl,
        target: undefined,
        rel: undefined,
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
        text: text?.includes('(opens in a new tab)')
          ? text
          : `${text} (opens in a new tab)`.trim(),
      };
    default:
      return {
        url: formattedUrl,
        target: '_blank',
        rel: buildRel(['noopener', 'noreferrer', 'nofollow', 'external'], rel),
        externality,
        text: text?.includes('(opens in a new tab)')
          ? text
          : `${text} (opens in a new tab)`.trim(),
      };
  }
}
