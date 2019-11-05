import React, { useEffect, useState } from 'react';
import { PublicationSubjectMeta } from '@common/modules/full-table/services/tableBuilderService';
import FootnotesList from './FootnotesList';
import FootnoteForm, { FootnoteFormConfig } from './FootnoteForm';
import { dummyFootnoteMeta, dummyFootnotes } from './dummyFootnoteData';

interface Props {
  publicationId: string;
  releaseId: string;
}

export interface FootnoteMeta {
  [key: number /* subjectId */]: {
    subjectId: number;
    subjectName: string;
    indicators: PublicationSubjectMeta['indicators'];
    filters: PublicationSubjectMeta['filters'];
  };
}

export interface Footnote {
  id?: string;
  content: string;
  subjects?: number[];
  indicators?: number[];
  filterGroups?: number[];
  filters?: number[];
  filterItems?: number[];
}

const ReleaseFootnotesSection = ({ publicationId, releaseId }: Props) => {
  const [footnoteMeta, setFootnoteMeta] = useState<FootnoteMeta>();
  const [footnotes, setFootnotes] = useState<Footnote[]>([]);
  const [footnoteForm, _setFootnoteForm] = useState<FootnoteFormConfig>({
    state: 'cancel',
  });

  useEffect(() => {
    setFootnoteMeta(dummyFootnoteMeta);
    setFootnotes(dummyFootnotes);
  }, [publicationId, releaseId]);

  const footnoteFormControls = {
    footnoteForm,
    create: () => _setFootnoteForm({ state: 'create' }),
    edit: (footnote: Footnote) => {
      _setFootnoteForm({ state: 'edit', footnote });
    },
    cancel: () => _setFootnoteForm({ state: 'cancel' }),
    save: (footnote: Footnote, footnoteId?: string) => {
      console.log(
        `updating footnote: ${footnoteId} with ${JSON.stringify(footnote)}`,
      );
    },
  };

  return (
    <>
      <h2>Footnotes</h2>
      <FootnoteForm
        {...footnoteForm}
        footnote={undefined}
        onOpen={footnoteFormControls.create}
        onCancel={footnoteFormControls.cancel}
        onSubmit={footnoteFormControls.save}
        isFirst={!footnotes.length}
      />
      {footnoteMeta && (
        <FootnotesList
          footnotes={footnotes}
          footnoteMeta={footnoteMeta}
          footnoteFormControls={footnoteFormControls}
        />
      )}
    </>
  );
};

export default ReleaseFootnotesSection;
