import {
  Footnote,
  FootnoteMeta,
  FootnoteProps,
  FootnoteMetaGetters,
} from '@admin/services/release/edit-release/footnotes/types';
import footnotesService from '@admin/services/release/edit-release/footnotes/service';
import { generateFootnoteMetaMap } from '@admin/services/release/edit-release/footnotes/util';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { useEffect, useState } from 'react';
import FootnotesList from './FootnotesList';
import FootnoteForm, { FootnoteFormConfig } from './FootnoteForm';
import { dummyFootnoteMeta, dummyFootnotes } from './dummyFootnoteData';

interface Props {
  publicationId: string;
  releaseId: string;
}

const ReleaseFootnotesSection = ({ publicationId, releaseId }: Props) => {
  const [footnoteMeta, setFootnoteMeta] = useState<FootnoteMeta>();
  const [footnotes, setFootnotes] = useState<Footnote[]>([]);
  const [footnoteForm, _setFootnoteForm] = useState<FootnoteFormConfig>({
    state: 'cancel',
  });
  const [footnoteMetaGetters, setFootnoteMetaGetters] = useState<
    FootnoteMetaGetters
  >();

  useEffect(() => {
    setFootnoteMeta(dummyFootnoteMeta);
    setFootnotes(dummyFootnotes);
    setFootnoteMetaGetters(generateFootnoteMetaMap(dummyFootnoteMeta));
  }, [publicationId, releaseId]);

  const footnoteFormControls = {
    footnoteForm,
    create: () => _setFootnoteForm({ state: 'create' }),
    edit: (footnote: Footnote) => {
      _setFootnoteForm({ state: 'edit', footnote });
    },
    cancel: () => _setFootnoteForm({ state: 'cancel' }),
    save: (footnote: FootnoteProps, footnoteId?: number) => {
      console.log(
        `updating footnote: ${footnoteId} with ${JSON.stringify(footnote)}`,
      );
      _setFootnoteForm({ state: 'cancel' });
    },
    delete: footnotesService.deleteFootnote,
  };

  return (
    <>
      <h2>Footnotes</h2>
      {!footnoteMeta || !footnoteMetaGetters ? (
        <LoadingSpinner />
      ) : (
        <>
          <FootnoteForm
            {...footnoteForm}
            footnote={undefined}
            onOpen={footnoteFormControls.create}
            onCancel={footnoteFormControls.cancel}
            onSubmit={footnoteFormControls.save}
            isFirst={!footnotes.length}
            footnoteMeta={footnoteMeta}
            footnoteMetaGetters={footnoteMetaGetters}
          />
          {footnoteMeta && (
            <FootnotesList
              footnotes={footnotes}
              footnoteMeta={footnoteMeta}
              footnoteMetaGetters={footnoteMetaGetters}
              footnoteFormControls={footnoteFormControls}
            />
          )}
        </>
      )}
    </>
  );
};

export default ReleaseFootnotesSection;
