import {
  Footnote,
  FootnoteProps,
} from '@admin/services/release/edit-release/footnotes/types';
import footnotesService from '@admin/services/release/edit-release/footnotes/service';
import Link from '@admin/components/Link';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ModalConfirm from '@common/components/ModalConfirm';
import React, { useEffect, useState } from 'react';
import FootnotesList from '@admin/components/footnotes/FootnotesList';
import FootnoteForm, {
  FootnoteFormConfig,
} from '@admin/components/footnotes/form/FootnoteForm';
import { FootnotesData } from '@admin/pages/release/edit-release/data/ReleaseDataPage';

interface Props {
  publicationId: string;
  releaseId: string;
  footnotesPropData?: FootnotesData;
  onSubmit: (footnotesData: FootnotesData) => void;
  getFootnoteData: () => void;
}

const ReleaseFootnotesSection = ({
  footnotesPropData: footnotesData,
  onSubmit,
  getFootnoteData,
  handleApiErrors,
}: Props & ErrorControlProps) => {
  const [loading, setLoading] = useState<boolean>(true);
  const [footnoteForm, _setFootnoteForm] = useState<FootnoteFormConfig>({
    state: 'cancel',
  });

  const [footnoteToBeDeleted, setFootnoteToBeDeleted] = useState<
    Footnote | undefined
  >();

  useEffect(() => {
    if (footnotesData) {
      setLoading(footnotesData.loading);
    }
  }, [footnotesData]);

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
        setLoading(true);
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
              setLoading(false);
            }
          })
          .catch(handleApiErrors);
      } else {
        setLoading(true);
        footnotesService
          .createFootnote(footnote)
          .then((newFootnote: Footnote) => {
            onSubmit({
              ...footnotesData,
              footnotes: [...footnotesData.footnotes, newFootnote],
            });
            setLoading(false);
          })
          .catch(handleApiErrors);
      }
      _setFootnoteForm({ state: 'cancel' });
    },
    delete: setFootnoteToBeDeleted,
  };

  return (
    <>
      <h2>Footnotes</h2>
      {footnotesData === undefined ||
        (!footnotesData.hasSufficientData && (
          <p>
            Before footnotes can be created, relevant data files need to be
            uploaded. That can be done in the{' '}
            <Link to="#data-upload">Data uploads section</Link>.
          </p>
        ))}
      {footnotesData && loading && <LoadingSpinner />}
      {footnotesData &&
        !loading &&
        footnotesData &&
        footnotesData.hasSufficientData &&
        footnotesData.footnoteMeta &&
        footnotesData.footnoteMetaGetters && (
          <>
            {footnotesData.canUpdateRelease && (
              <FootnoteForm
                {...footnoteForm}
                footnote={undefined}
                onOpen={footnoteFormControls.create}
                onCancel={footnoteFormControls.cancel}
                onSubmit={footnoteFormControls.save}
                isFirst={
                  footnotesData.footnotes &&
                  footnotesData.footnotes.length === 0
                }
                footnoteMeta={footnotesData.footnoteMeta}
                footnoteMetaGetters={footnotesData.footnoteMetaGetters}
              />
            )}
            {footnotesData.footnoteMeta && (
              <>
                <FootnotesList
                  footnotes={footnotesData.footnotes}
                  footnoteMeta={footnotesData.footnoteMeta}
                  footnoteMetaGetters={footnotesData.footnoteMetaGetters}
                  footnoteFormControls={footnoteFormControls}
                  canUpdateRelease={footnotesData.canUpdateRelease}
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
                        .then(getFootnoteData)
                        .catch(handleApiErrors);
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

export default withErrorControl(ReleaseFootnotesSection);
