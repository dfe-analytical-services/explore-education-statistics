import {
  Footnote,
  FootnoteMeta,
  FootnoteProps,
  FootnoteMetaGetters,
} from '@admin/services/release/edit-release/footnotes/types';
import footnotesService from '@admin/services/release/edit-release/footnotes/service';
import { generateFootnoteMetaMap } from '@admin/services/release/edit-release/footnotes/util';
import Link from '@admin/components/Link';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
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
  const [footnoteToBeDeleted, setFootnoteToBeDeleted] = useState<
    Footnote | undefined
  >();
  const [hasSufficientData, setHasSufficientData] = useState<boolean>(true);

  function getFootnoteData() {
    setLoading(true);
    footnotesService
      .getReleaseFootnoteData(releaseId)
      .then(({ meta, footnotes: footnotesList }) => {
        setFootnoteMeta(meta);
        setHasSufficientData(!!Object.keys(meta).length);
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
        setLoading(true);
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
            setLoading(false);
          }
        });
      } else {
        setLoading(true);
        footnotesService
          .createFootnote(footnote)
          .then((newFootnote: Footnote) => {
            setFootnotes([...footnotes, newFootnote]);
            setLoading(false);
          });
      }
      _setFootnoteForm({ state: 'cancel' });
    },
    delete: setFootnoteToBeDeleted,
  };

  return (
    <>
      <h2>Footnotes</h2>
      {!hasSufficientData && (
        <p>
          Before footnotes can be created, relevant data files need to be
          uploaded. That can be done in the{' '}
          <Link to="#data-upload">Data uploads section</Link>.
        </p>
      )}
      {loading && <LoadingSpinner />}
      {!loading && hasSufficientData && footnoteMeta && footnoteMetaGetters && (
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
            <>
              <FootnotesList
                footnotes={footnotes}
                footnoteMeta={footnoteMeta}
                footnoteMetaGetters={footnoteMetaGetters}
                footnoteFormControls={footnoteFormControls}
              />
              {typeof footnoteToBeDeleted !== 'undefined' && (
                <ModalConfirm
                  title="Confirm deletion of footnote"
                  onExit={() => setFootnoteToBeDeleted(undefined)}
                  onCancel={() => setFootnoteToBeDeleted(undefined)}
                  onConfirm={() => {
                    footnotesService
                      .deleteFootnote((footnoteToBeDeleted as Footnote).id)
                      .then(() => setFootnoteToBeDeleted(undefined))
                      .then(getFootnoteData);
                  }}
                >
                  The footnote:
                  <p className="govuk-inset-text">
                    {(footnoteToBeDeleted as Footnote).content}
                  </p>
                </ModalConfirm>
              )}
            </>
          )}
        </>
      )}
    </>
  );
};

export default ReleaseFootnotesSection;
