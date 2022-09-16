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
import React, { ReactElement, useEffect, useMemo, useRef } from 'react';
import styles from './ContentHtml.module.scss';

export interface ContentHtmlProps {
  className?: string;
  html: string;
  sanitizeOptions?: SanitizeHtmlOptions;
  testId?: string;
  getGlossaryEntry?: (slug: string) => Promise<GlossaryEntry>;
  trackContentLinks?: (url: string, newTab?: boolean) => void;
  trackGlossaryLinks?: (glossaryEntrySlug: string) => void;
}

export default function ContentHtml({
  className,
  html,
  sanitizeOptions,
  testId,
  getGlossaryEntry,
  trackContentLinks,
  trackGlossaryLinks,
}: ContentHtmlProps) {
  const { isMounted } = useMounted();
  const contentAreaRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const currentRef = contentAreaRef.current;

    const handleClick = (event: MouseEvent) => {
      const element = event.target as HTMLAnchorElement;

      if (trackContentLinks && element.tagName === 'A' && element.href) {
        event.preventDefault();
        trackContentLinks(element.href, element.target === '_blank');
      }
    };

    currentRef?.addEventListener('click', handleClick);

    return () => {
      currentRef?.removeEventListener('click', handleClick);
    };
  }, [contentAreaRef, trackContentLinks]);

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
            href={node.attribs.href}
            getEntry={getGlossaryEntry}
            onToggle={trackGlossaryLinks}
          >
            {domToReact(node.children)}
          </GlossaryEntryButton>
        ) : undefined;
      }

      if (node.name === 'figure' && node.attribs.class === 'table') {
        return renderTable(node);
      }

      return undefined;
    },
  });

  return (
    <div
      className={classNames('dfe-content', className)}
      data-testid={testId}
      ref={contentAreaRef}
    >
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
