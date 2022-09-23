import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import Modal from '@common/components/Modal';
import { GlossaryEntry } from '@common/services/types/glossary';
import sanitizeHtml from '@common/utils/sanitizeHtml';
import parseHtmlString from 'html-react-parser';
import React, { ReactNode, useState } from 'react';

interface Props {
  children: ReactNode;
  href: string;
  getEntry: (slug: string) => Promise<GlossaryEntry>;
  onToggle?: (slug: string) => void;
}

export default function GlossaryEntryButton({
  children,
  href,
  getEntry,
  onToggle,
}: Props) {
  const [glossaryEntry, setGlossaryEntry] = useState<GlossaryEntry>();

  return (
    <>
      <ButtonText
        onClick={async () => {
          const slug = href.split('#')[1];
          const newGlossaryEntry = await getEntry(slug);

          setGlossaryEntry(newGlossaryEntry);

          onToggle?.(slug);
        }}
      >
        {children} <InfoIcon description="(show glossary term definition)" />
      </ButtonText>

      {glossaryEntry && (
        <Modal
          title={glossaryEntry.title}
          onExit={() => setGlossaryEntry(undefined)}
        >
          {parseHtmlString(sanitizeHtml(glossaryEntry.body))}

          <Button onClick={() => setGlossaryEntry(undefined)}>Close</Button>
        </Modal>
      )}
    </>
  );
}
