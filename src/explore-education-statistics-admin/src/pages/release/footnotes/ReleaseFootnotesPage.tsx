import Link from '@admin/components/Link';
import generateFootnoteMetaMap, {
  FootnoteMetaGetters,
} from '@admin/pages/release/data/utils/generateFootnoteMetaMap';
import FootnotesList from '@admin/pages/release/footnotes/components/FootnotesList';
import FootnoteForm, {
  FootnoteFormConfig,
} from '@admin/pages/release/footnotes/components/form/FootnoteForm';
import {
  releaseDataRoute,
  ReleaseRouteParams,
} from '@admin/routes/releaseRoutes';
import footnotesService, {
  BaseFootnote,
  Footnote,
  FootnoteMeta,
} from '@admin/services/footnoteService';
import permissionService from '@admin/services/permissionService';
import ModalConfirm from '@common/components/ModalConfirm';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import React, { useState } from 'react';
import { generatePath, RouteComponentProps } from 'react-router';

interface FootnotesData {
  footnoteMeta: FootnoteMeta;
  footnotes: Footnote[];
  footnoteMetaGetters: FootnoteMetaGetters;
}

const ReleaseFootnotesPage = ({
  match,
}: RouteComponentProps<ReleaseRouteParams>) => {
  const { publicationId, releaseId } = match.params;

  const [footnoteForm, _setFootnoteForm] = useState<FootnoteFormConfig>({
    state: 'cancel',
  });

  const [footnoteToBeDeleted, setFootnoteToBeDeleted] = useState<
    Footnote | undefined
  >();

  const { value: canUpdateRelease = false } = useAsyncHandledRetry(
    () => permissionService.canUpdateRelease(releaseId),
    [releaseId],
  );

  const {
    value: footnotesData,
    retry: fetchFootnotesData,
    setState: setFootnotesData,
  } = useAsyncHandledRetry<FootnotesData>(async () => {
    const {
      meta,
      footnotes: footnotesList,
    } = await footnotesService.getReleaseFootnoteData(releaseId);

    return {
      footnoteMeta: meta,
      footnotes: footnotesList,
      footnoteMetaGetters: generateFootnoteMetaMap(meta),
    };
  }, [releaseId]);

  const footnoteFormControls = {
    footnoteForm,
    create: () => _setFootnoteForm({ state: 'create' }),
    edit: (footnote: Footnote) => {
      _setFootnoteForm({ state: 'edit', footnote });
    },
    cancel: () => _setFootnoteForm({ state: 'cancel' }),
    save: (footnote: BaseFootnote, footnoteId?: string) => {
      if (!footnotesData) return;
      if (footnoteId) {
        footnotesService
          .updateFootnote(releaseId, footnoteId, footnote)
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
              setFootnotesData({
                value: {
                  ...footnotesData,
                  footnotes: updatedFootnotes,
                },
              });
            }
          });
      } else {
        footnotesService
          .createFootnote(releaseId, footnote)
          .then((newFootnote: Footnote) => {
            setFootnotesData({
              value: {
                ...footnotesData,
                footnotes: [...footnotesData.footnotes, newFootnote],
              },
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

      {!canUpdateRelease && (
        <p>This release has been approved, and can no longer be updated.</p>
      )}

      {footnotesData.footnoteMeta && (
        <>
          {canUpdateRelease && (
            <FootnoteForm
              {...footnoteForm}
              footnote={undefined}
              onOpen={footnoteFormControls.create}
              onCancel={footnoteFormControls.cancel}
              onSubmit={footnoteFormControls.save}
              isFirst={!footnotesData.footnotes?.length}
              footnoteMeta={footnotesData.footnoteMeta}
              footnoteMetaGetters={footnotesData.footnoteMetaGetters}
            />
          )}
          <>
            <FootnotesList
              {...footnotesData}
              canUpdateRelease={canUpdateRelease}
              footnoteFormControls={footnoteFormControls}
            />
            {typeof footnoteToBeDeleted !== 'undefined' && (
              <ModalConfirm
                title="Confirm deletion of footnote"
                onExit={() => setFootnoteToBeDeleted(undefined)}
                onCancel={() => setFootnoteToBeDeleted(undefined)}
                onConfirm={() => {
                  footnotesService
                    .deleteFootnote(
                      releaseId,
                      (footnoteToBeDeleted as Footnote).id,
                    )
                    .then(() => setFootnoteToBeDeleted(undefined))
                    .then(fetchFootnotesData);
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
        <Link
          to={generatePath<ReleaseRouteParams>(releaseDataRoute.path, {
            publicationId,
            releaseId,
          })}
        >
          Data uploads section
        </Link>
        .
      </p>
    </>
  );
};

export default ReleaseFootnotesPage;
