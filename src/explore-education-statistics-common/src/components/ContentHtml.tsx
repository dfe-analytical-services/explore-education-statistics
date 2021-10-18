import sanitizeHtml, { SanitizeHtmlOptions } from '@common/utils/sanitizeHtml';
import parseHtmlString, { DOMNode, domToReact } from 'html-react-parser';
import { Element } from 'domhandler/lib/node';
import classNames from 'classnames';
import React, { useMemo, useState } from 'react';
import { GlossaryEntry } from '@common/services/types/glossary';
import Modal from '@common/components/Modal';
import InfoIcon from '@common/components/InfoIcon';
import useMounted from '@common/hooks/useMounted';
import ButtonText from '@common/components/ButtonText';
import Button from '@common/components/Button';

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
  const { isMounted } = useMounted();

  const [glossaryEntry, setGlossaryEntry] = useState<GlossaryEntry | undefined>(
    undefined,
  );

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
        const linkText = domToReact(node.children);
        const glossaryEntrySlug = node.attribs.href.split('#')[1];
        return isMounted ? (
          <>
            <ButtonText
              onClick={async () => {
                const newGlossaryEntry = await getGlossaryEntry(
                  glossaryEntrySlug,
                );
                setGlossaryEntry(newGlossaryEntry);
              }}
            >
              {linkText}{' '}
              <InfoIcon description="(show glossary term definition)" />
            </ButtonText>
          </>
        ) : (
          <a href={node.attribs.href}>{linkText}</a>
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
      {glossaryEntry && (
        <Modal
          title={glossaryEntry.title}
          onExit={() => setGlossaryEntry(undefined)}
        >
          {parseHtmlString(sanitizeHtml(glossaryEntry.body, sanitizeOptions))}
          <Button
            onClick={() => {
              setGlossaryEntry(undefined);
            }}
          >
            Close
          </Button>
        </Modal>
      )}
    </>
  );
};

export default ContentHtml;
