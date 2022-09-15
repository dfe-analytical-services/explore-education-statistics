import GlossaryEntryButton from '@common/components/GlossaryEntryButton';
import useMounted from '@common/hooks/useMounted';
import { GlossaryEntry } from '@common/services/types/glossary';
import sanitizeHtml, { SanitizeHtmlOptions } from '@common/utils/sanitizeHtml';
import classNames from 'classnames';
import { Element } from 'domhandler/lib/node';
import parseHtmlString, { DOMNode, domToReact } from 'html-react-parser';
import React, { useEffect, useMemo, useRef } from 'react';

export interface ContentHtmlProps {
  className?: string;
  html: string;
  sanitizeOptions?: SanitizeHtmlOptions;
  testId?: string;
  getGlossaryEntry?: (slug: string) => Promise<GlossaryEntry>;
  trackContentLinks?: (url: string, newTab?: boolean) => void;
  trackGlossaryLinks?: (glossaryEntrySlug: string) => void;
}

const ContentHtml = ({
  className,
  html,
  sanitizeOptions,
  testId,
  getGlossaryEntry,
  trackContentLinks,
  trackGlossaryLinks,
}: ContentHtmlProps) => {
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
};

export default ContentHtml;
