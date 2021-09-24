import sanitizeHtml, { SanitizeHtmlOptions } from '@common/utils/sanitizeHtml';
import parseHtmlString, { DOMNode, domToReact } from 'html-react-parser';
import { Element } from 'domhandler/lib/node';
import classNames from 'classnames';
import React, { useMemo, useState } from 'react';
import { GlossaryEntry } from '@common/services/types/glossary';
import Modal from '@common/components/Modal';
import styles from './ContentHtml.module.scss';

export interface ContentHtmlProps {
  className?: string;
  html: string;
  sanitizeOptions?: SanitizeHtmlOptions;
  testId?: string;
  getGlossaryEntry?: (slug: string) => Promise<GlossaryEntry>;
}

const ContentHtml = ({
  className,
  html,
  sanitizeOptions,
  testId,
  getGlossaryEntry,
}: ContentHtmlProps) => {
  const [displayGlossaryEntry, setDisplayGlossaryEntry] = useState(false);
  const [glossaryEntry, setGlossaryEntry] = useState({
    title: 'Glossary entry not retrieved',
    slug: '',
    body: '',
    link: '',
  });

  const cleanHtml = useMemo(() => {
    return sanitizeHtml(html, sanitizeOptions);
  }, [html, sanitizeOptions]);

  const parsedContent = parseHtmlString(cleanHtml, {
    replace: (node: DOMNode) => {
      if (
        getGlossaryEntry &&
        node instanceof Element &&
        node.name === 'a' &&
        node.attribs &&
        Object.keys(node.attribs).includes('data-glossary')
      ) {
        return (
          <>
            <a href={node.attribs.href}>{domToReact(node.children)}</a>{' '}
            <a
              className={styles.infoIcon}
              data-testid="glossary-info-icon"
              href="#"
              onClick={async event => {
                event.preventDefault();
                const glossaryEntrySlug = node.attribs.href.split('#')[1];
                const newGlossaryEntry = await getGlossaryEntry(
                  glossaryEntrySlug,
                );
                setGlossaryEntry({
                  title: newGlossaryEntry.title,
                  slug: newGlossaryEntry.slug,
                  body: newGlossaryEntry.body,
                  link: node.attribs.href,
                });
                setDisplayGlossaryEntry(true);
              }}
            >
              ?
            </a>
          </>
        );
      }
      return undefined;
    },
  });

  return (
    <>
      <div
        className={classNames('dfe-content', className)}
        data-testid={testId}
      >
        {parsedContent}
      </div>
      {displayGlossaryEntry && (
        <Modal
          title={glossaryEntry.title}
          onExit={() => setDisplayGlossaryEntry(false)}
        >
          {parseHtmlString(sanitizeHtml(glossaryEntry.body))}
          <p>
            <a
              href="#"
              onClick={event => {
                event.preventDefault();
                setDisplayGlossaryEntry(false);
              }}
            >
              Close
            </a>
          </p>
        </Modal>
      )}
    </>
  );
};

export default ContentHtml;
