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

interface Props {
  publicationId: string;
  releaseId: string;
}

const ReleaseFootnotesSection = ({ publicationId, releaseId }: Props) => {
  const [loading, setLoading] = useState<boolean>(true);
  const [footnoteMeta, setFootnoteMeta] = useState<FootnoteMeta>();
  const [footnotes, setFootnotes] = useState<Footnote[]>([]);
  const [footnoteForm, _setFootnoteForm] = useState<FootnoteFormConfig>({
    state: 'cancel',
  });
  const [footnoteMetaGetters, setFootnoteMetaGetters] = useState<
    FootnoteMetaGetters
  >();

  function getFootnoteData() {
    setLoading(true);
    footnotesService
      .getReleaseFootnoteData(releaseId)
      .then(({ meta, footnotes: footnotesList }) => {
        setFootnoteMeta(meta);
        setFootnotes(footnotesList);
        setFootnoteMetaGetters(generateFootnoteMetaMap(meta));
        setLoading(false);
      });
  }

  useEffect(() => {
    getFootnoteData();
  }, [publicationId, releaseId]);

  const footnoteFormControls = {
    footnoteForm,
    create: () => _setFootnoteForm({ state: 'create' }),
    edit: (footnote: Footnote) => {
      _setFootnoteForm({ state: 'edit', footnote });
    },
    cancel: () => _setFootnoteForm({ state: 'cancel' }),
    save: (footnote: FootnoteProps, footnoteId?: number) => {
      if (footnoteId) {
        footnotesService.updateFootnote(footnoteId, footnote).then(() => {
          const index = footnotes.findIndex((searchElement: Footnote) => {
            return footnoteId === searchElement.id;
          });
          if (index > -1) {
            const updatedFootnotes = [...footnotes];
            updatedFootnotes[index] = {
              ...footnote,
              id: footnoteId,
            };
            setFootnotes(updatedFootnotes);
          }
        });
      } else {
        footnotesService
          .createFootnote(footnote)
          .then((newFootnote: Footnote) => {
            setFootnotes([...footnotes, newFootnote]);
          });
      }
      _setFootnoteForm({ state: 'cancel' });
    },
    delete: (footnoteId: number) => {
      footnotesService.deleteFootnote(footnoteId).then(getFootnoteData);
    },
  };

  return (
    <>
      <h2>Footnotes</h2>
      {loading || !footnoteMeta || !footnoteMetaGetters ? (
        <LoadingSpinner />
      ) : (
        <>
          <FootnoteForm
            {...footnoteForm}
            footnote={undefined}
            onOpen={footnoteFormControls.create}
            onCancel={footnoteFormControls.cancel}
            onSubmit={footnoteFormControls.save}
            isFirst={footnotes && footnotes.length === 0}
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
