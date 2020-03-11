import FootnotesList from '@admin/components/footnotes/FootnotesList';
import FootnoteForm, {
  FootnoteFormConfig,
} from '@admin/components/footnotes/form/FootnoteForm';
import Link from '@admin/components/Link';
import { FootnotesData } from '@admin/pages/release/edit-release/data/ReleaseDataPage';
import footnotesService from '@admin/services/release/edit-release/footnotes/service';
import {
  Footnote,
  FootnoteProps,
} from '@admin/services/release/edit-release/footnotes/types';
import ModalConfirm from '@common/components/ModalConfirm';
import React, { useState } from 'react';

interface Props {
  publicationId: string;
  releaseId: string;
  footnotesData?: FootnotesData;
  onSubmit: (footnotesData: FootnotesData) => void;
  onDelete: () => void;
}

const ReleaseFootnotesSection = ({
  footnotesData,
  onSubmit,
  onDelete,
}: Props) => {
  const [footnoteForm, _setFootnoteForm] = useState<FootnoteFormConfig>({
    state: 'cancel',
  });

  const [footnoteToBeDeleted, setFootnoteToBeDeleted] = useState<
    Footnote | undefined
  >();

  const footnoteFormControls = {
    footnoteForm,
    create: () => _setFootnoteForm({ state: 'create' }),
    edit: (footnote: Footnote) => {
      _setFootnoteForm({ state: 'edit', footnote });
    },
    cancel: () => _setFootnoteForm({ state: 'cancel' }),
    save: (footnote: FootnoteProps, footnoteId?: string) => {
      if (!footnotesData) return;
      if (footnoteId) {
        footnotesService
          .updateFootnote(footnoteId, footnote)
          .then(updatedFootnote => {
            const index = footnotesData.footnotes.findIndex(
              (searchElement: Footnote) => {
                return footnoteId === searchElement.id;
              },
            );
            if (index > -1) {
              const updatedFootnotes = [...footnotesData.footnotes];
              updatedFootnotes[index] = {
                ...updatedFootnote,
                id: footnoteId,
              };
              onSubmit({
                ...footnotesData,
                footnotes: updatedFootnotes,
              });
            }
          });
      } else {
        footnotesService
          .createFootnote(footnote)
          .then((newFootnote: Footnote) => {
            onSubmit({
              ...footnotesData,
              footnotes: [...footnotesData.footnotes, newFootnote],
            });
          });
      }
      _setFootnoteForm({ state: 'cancel' });
    },
    delete: setFootnoteToBeDeleted,
  };

  return footnotesData && !!Object.keys(footnotesData.footnoteMeta).length ? (
    <>
      <h2>Footnotes</h2>

      {footnotesData.footnoteMeta && (
        <>
          {footnotesData.canUpdateRelease && (
            <FootnoteForm
              {...footnoteForm}
              footnote={undefined}
              onOpen={footnoteFormControls.create}
              onCancel={footnoteFormControls.cancel}
              onSubmit={footnoteFormControls.save}
              isFirst={
                footnotesData.footnotes && footnotesData.footnotes.length === 0
              }
              footnoteMeta={footnotesData.footnoteMeta}
              footnoteMetaGetters={footnotesData.footnoteMetaGetters}
            />
          )}
          <>
            <FootnotesList
              {...footnotesData}
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
                    .then(onDelete);
                }}
              >
                The footnote:
                <p className="govuk-inset-text">
                  {(footnoteToBeDeleted as Footnote).content}
                </p>
              </ModalConfirm>
            )}
          </>
        </>
      )}
    </>
  ) : (
    <>
      <h2>Footnotes</h2>
      <p>
        Before footnotes can be created, relevant data files need to be
        uploaded. That can be done in the{' '}
        <Link to="#data-upload">Data uploads section</Link>.
      </p>
    </>
  );
};

export default ReleaseFootnotesSection;
