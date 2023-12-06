import GlossaryEntryButton from '@common/components/GlossaryEntryButton';
import useMounted from '@common/hooks/useMounted';
import { GlossaryEntry } from '@common/services/types/glossary';
import sanitizeHtml, { SanitizeHtmlOptions } from '@common/utils/sanitizeHtml';
import classNames from 'classnames';
import { Element } from 'domhandler/lib/node';
import parseHtmlString, {
  DOMNode,
  domToReact,
  attributesToProps,
} from 'html-react-parser';
import React, { ReactElement, useMemo } from 'react';
import styles from './ContentHtml.module.scss';

export interface ContentHtmlProps {
  className?: string;
  getGlossaryEntry?: (slug: string) => Promise<GlossaryEntry>;
  html: string;
  sanitizeOptions?: SanitizeHtmlOptions;
  testId?: string;
  trackGlossaryLinks?: (glossaryEntrySlug: string) => void;
  transformFeaturedTableLinks?: (url: string, text: string) => void;
}

export default function ContentHtml({
  className,
  getGlossaryEntry,
  html,
  sanitizeOptions,
  testId,
  trackGlossaryLinks,
  transformFeaturedTableLinks,
}: ContentHtmlProps) {
  const { isMounted } = useMounted();

  const cleanHtml = useMemo(() => {
    return sanitizeHtml(html, sanitizeOptions);
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

      if (node.name === 'figure' && node.attribs.class === 'table') {
        return renderTable(node);
      }

      return undefined;
    },
  });

  return (
    <div className={classNames('dfe-content', className)} data-testid={testId}>
      {parsedContent}
    </div>
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

  const figcaption = children.find(
    child => child instanceof Element && child.name === 'figcaption',
  ) as Element | undefined;

  if (!table || !figcaption) {
    return undefined;
  }

  return (
    <div className={styles.tableContainer}>
      <table
        // eslint-disable-next-line react/jsx-props-no-spreading
        {...attributesToProps(table.attribs)}
      >
        <caption>{domToReact(figcaption.children)}</caption>
        {domToReact(table.children, { trim: true })}
      </table>
    </div>
  );
}
