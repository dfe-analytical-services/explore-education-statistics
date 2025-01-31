import ButtonText from '@common/components/ButtonText';
import InfoIcon from '@common/components/InfoIcon';
import Modal from '@common/components/Modal';
import { GlossaryEntry } from '@common/services/types/glossary';
import sanitizeHtml from '@common/utils/sanitizeHtml';
import parseHtmlString from 'html-react-parser';
import React, { ReactNode, useState } from 'react';

interface Props {
  children: ReactNode;
  getEntry: (slug: string) => Promise<GlossaryEntry>;
  href: string;
  onToggle?: (slug: string) => void;
}

export default function GlossaryEntryButton({
  children,
  getEntry,
  href,
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
          open={!!glossaryEntry}
          showClose
          title={glossaryEntry.title}
          onExit={() => setGlossaryEntry(undefined)}
        >
          {parseHtmlString(sanitizeHtml(glossaryEntry.body))}
        </Modal>
      )}
    </>
  );
}
