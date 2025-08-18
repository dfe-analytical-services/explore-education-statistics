import GlossaryEntryButton from '@common/components/GlossaryEntryButton';
import styles from '@common/components/ContentHtml.module.scss';
import formatContentLink from '@common/utils/url/formatContentLink';
import useMounted from '@common/hooks/useMounted';
import { GlossaryEntry } from '@common/services/types/glossary';
import sanitizeHtml, {
  SanitizeHtmlOptions,
  defaultSanitizeOptions,
} from '@common/utils/sanitizeHtml';
import allowedHosts from '@common/utils/url/allowedHosts';
import classNames from 'classnames';
import { Element } from 'domhandler/lib/node';
import parseHtmlString, {
  DOMNode,
  domToReact,
  attributesToProps,
} from 'html-react-parser';
import React, { ReactElement, useMemo } from 'react';

export interface ContentHtmlProps {
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
      if (
        getGlossaryEntry &&
        node.name === 'a' &&
        typeof node.attribs['data-glossary'] !== 'undefined'
      ) {
        return isMounted ? (
          <GlossaryEntryButton
            getEntry={getGlossaryEntry}
            href={node.attribs.href}
            onToggle={trackGlossaryLinks}
          >
            {domToReact(node.children)}
          </GlossaryEntryButton>
        ) : undefined;
      }

      if (
        transformFeaturedTableLinks &&
        node.name === 'a' &&
        typeof node.attribs['data-featured-table'] !== 'undefined'
      ) {
        const text = domToReact(node.children);
        return isMounted && typeof text === 'string'
          ? transformFeaturedTableLinks(node.attribs.href, text)
          : undefined;
      }

      if (formatLinks && node.name === 'a') {
        const urlString = formatContentLink(node.attribs.href);

        try {
          const url = new URL(urlString);
          const text = domToReact(node.children);

          return !allowedHosts.includes(url.host) &&
            (url.protocol === 'http:' || url.protocol === 'https:') &&
            typeof node.attribs['data-featured-table'] === 'undefined' ? (
            <a href={urlString} target="_blank" rel="noopener noreferrer">
              {text} (opens in a new tab)
            </a>
          ) : (
            <a href={urlString}>{text}</a>
          );
        } catch {
          return false;
        }
      }

      if (node.name === 'figure' && node.attribs.class === 'table') {
        return renderTable(node);
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
        {figcaption && <caption>{domToReact(figcaption.children)}</caption>}
        {domToReact(table.children, { trim: true })}
      </table>
    </div>
  );
}
