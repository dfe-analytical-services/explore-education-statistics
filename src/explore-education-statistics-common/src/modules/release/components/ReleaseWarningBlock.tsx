import WarningMessage from '@common/components/WarningMessage';
import { ContentBlock } from '@common/services/types/blocks';
import sanitizeHtml, {
  releaseWarningBlockSanitizeOptions,
} from '@common/utils/sanitizeHtml';
import formatContentLinkUrl from '@common/utils/url/formatContentLinkUrl';
import getUrlAttributes from '@common/utils/url/getUrlAttributes';
import { isTag } from 'domhandler';
import parseHtmlString, {
  DOMNode,
  domToReact,
  HTMLReactParserOptions,
} from 'html-react-parser';
import React from 'react';

interface Props {
  block: ContentBlock;
}

// The html should only contain p and a elements.
// We need to strip out the p elements as the WarningMessage
// wraps the child content in <strong> tags, and process the
// links to add correct attributes and text.
export const releaseWarningBlockParseOptions: HTMLReactParserOptions = {
  replace: (node: DOMNode) => {
    if (!isTag(node)) {
      return undefined;
    }

    if (node.name === 'a') {
      const text = domToReact(
        node.children as DOMNode[],
        releaseWarningBlockParseOptions,
      );

      const urlString = formatContentLinkUrl(node.attribs.href);
      const urlAttributes = getUrlAttributes(urlString);

      if (urlAttributes && urlAttributes.isExternal) {
        const rel = 'noopener noreferrer nofollow';
        return (
          // eslint-disable-next-line react/jsx-no-target-blank
          <a
            href={urlString}
            target="_blank"
            rel={urlAttributes.isTrusted ? rel : `${rel} external`}
          >
            {text} (opens in new tab)
          </a>
        );
      }
      return <a href={urlString}>{text}</a>;
    }

    if (node.name === 'p') {
      const text = domToReact(
        node.children as DOMNode[],
        releaseWarningBlockParseOptions,
      );
      return (
        <>
          {text}
          <br />
        </>
      );
    }

    return undefined;
  },
};

const ReleaseWarningBlock = ({ block }: Props) => {
  const { body = '' } = block;

  const cleanHtml = sanitizeHtml(body, releaseWarningBlockSanitizeOptions);
  const parsedContent = parseHtmlString(
    cleanHtml,
    releaseWarningBlockParseOptions,
  );

  return (
    <WarningMessage className="govuk-!-margin-bottom-4">
      {parsedContent}
    </WarningMessage>
  );
};

export default ReleaseWarningBlock;
