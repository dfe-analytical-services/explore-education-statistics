import styles from '@common/components/ContentHtml.module.scss';
import GlossaryEntryButton from '@common/components/GlossaryEntryButton';
import generateIdFromHeading from '@common/components/util/generateIdFromHeading';
import useMounted from '@common/hooks/useMounted';
import { GlossaryEntry } from '@common/services/types/glossary';
import sanitizeHtml, {
  SanitizeHtmlOptions,
  defaultSanitizeOptions,
} from '@common/utils/sanitizeHtml';
import formatContentLinkUrl from '@common/utils/url/formatContentLinkUrl';
import getUrlAttributes from '@common/utils/url/getUrlAttributes';
import classNames from 'classnames';
import { Element } from 'domhandler/lib/node';
import parseHtmlString, {
  DOMNode,
  attributesToProps,
  domToReact,
} from 'html-react-parser';
import React, { ReactElement, useMemo, type JSX } from 'react';

export interface ContentHtmlProps {
  blockId?: string;
  className?: string;
  formatLinks?: boolean;
  getGlossaryEntry?: (slug: string) => Promise<GlossaryEntry>;
  html: string;
  sanitizeOptions?: SanitizeHtmlOptions;
  testId?: string;
  trackGlossaryLinks?: (glossaryEntrySlug: string) => void;
  transformFeaturedTableLinks?: (url: string, text: string) => void;
  wrapperElement?: keyof JSX.IntrinsicElements;
}

export default function ContentHtml({
  blockId,
  className,
  formatLinks = true,
  getGlossaryEntry,
  html,
  sanitizeOptions = defaultSanitizeOptions,
  testId,
  trackGlossaryLinks,
  transformFeaturedTableLinks,
  wrapperElement: Wrapper = 'div',
}: ContentHtmlProps) {
  const { isMounted } = useMounted();

  const cleanHtml = useMemo(() => {
    const opts: SanitizeHtmlOptions = {
      ...sanitizeOptions,
      transformTags: sanitizeOptions?.transformTags,
    };

    return sanitizeHtml(html, opts);
  }, [html, sanitizeOptions]);

  const parsedContent = parseHtmlString(cleanHtml, {
    replace: (node: DOMNode) => {
      if (!(node instanceof Element)) {
        return undefined;
      }

      // Links
      if (node.name === 'a') {
        const text = domToReact(node.children as DOMNode[]);

        // Glossary links
        if (
          getGlossaryEntry &&
          typeof node.attribs['data-glossary'] !== 'undefined'
        ) {
          return isMounted ? (
            <GlossaryEntryButton
              getEntry={getGlossaryEntry}
              href={node.attribs.href}
              onToggle={trackGlossaryLinks}
            >
              {text}
            </GlossaryEntryButton>
          ) : undefined;
        }

        // Featured table links
        if (
          transformFeaturedTableLinks &&
          typeof node.attribs['data-featured-table'] !== 'undefined'
        ) {
          return isMounted && typeof text === 'string'
            ? transformFeaturedTableLinks(node.attribs.href, text)
            : undefined;
        }

        // Standard links
        if (formatLinks) {
          // This is done when content is saved, but do here again to cover links that were
          // added before that change was made.
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
      }

      // Tables
      if (node.name === 'figure' && node.attribs.class === 'table') {
        return renderTable(node);
      }

      if (node.name === 'h3') {
        const text = domToReact(node.children as DOMNode[]);
        if (typeof text === 'string') {
          // generate an id optionally using 4 chars of the block id to ensure uniqueness
          <h3 id={generateIdFromHeading(text, blockId?.substring(0, 4))}>
            {text}
          </h3>;
        }
      }

      return undefined;
    },
  });

  return (
    <Wrapper
      className={classNames('dfe-content', className)}
      data-testid={testId}
    >
      {parsedContent}
    </Wrapper>
  );
}

/**
 * Fixes accessibility issues with table markup from CKEditor by
 * replacing the figure/figcaption implementation (which does not
 * get read out correctly) with a standard table/caption.
 */
function renderTable(element: Element): ReactElement | undefined {
  const { children } = element;

  const table = children.find(
    child => child instanceof Element && child.name === 'table',
  ) as Element | undefined;

  if (!table) {
    return undefined;
  }

  const figcaption = children.find(
    child => child instanceof Element && child.name === 'figcaption',
  ) as Element | undefined;

  return (
    // eslint-disable-next-line jsx-a11y/no-noninteractive-tabindex
    <div className={styles.tableContainer} tabIndex={0}>
      <table
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...attributesToProps(table.attribs)}
      >
        {figcaption && (
          <caption>{domToReact(figcaption.children as DOMNode[])}</caption>
        )}
        {domToReact(table.children as DOMNode[], { trim: true })}
      </table>
    </div>
  );
}
