import Link from '@admin/components/Link';
import {
  releaseAncillaryFileRoute,
  ReleaseDataFileRouteParams,
} from '@admin/routes/releaseRoutes';
import { AncillaryFile } from '@admin/services/releaseAncillaryFileService';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import ModalConfirm from '@common/components/ModalConfirm';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React from 'react';
import { generatePath } from 'react-router-dom';
import downloadReleaseFileSecurely from '@admin/pages/release/data/components/utils/downloadReleaseFileSecurely';

interface Props {
  canUpdateRelease?: boolean;
  file: AncillaryFile;
  publicationId: string;
  releaseVersionId: string;
  onCancelDelete?: () => void;
  onConfirmDelete: () => Promise<void>;
  onDelete?: () => void;
}

export default function AncillaryFileSummaryList({
  canUpdateRelease,
  file,
  publicationId,
  releaseVersionId,
  onCancelDelete,
  onConfirmDelete,
  onDelete,
}: Props) {
  return (
    <SummaryList>
      <SummaryListItem term="Title">{file.title}</SummaryListItem>

      <SummaryListItem term="File">
        <ButtonText
          onClick={() =>
            downloadReleaseFileSecurely({
              releaseVersionId,
              fileId: file.id,
              fileName: file.filename,
            })
          }
        >
          {file.filename}
        </ButtonText>
      </SummaryListItem>

      <SummaryListItem term="File size">
        {file.fileSize.size.toLocaleString()} {file.fileSize.unit}
      </SummaryListItem>

      <SummaryListItem term="Uploaded by">
        <a href={`mailto:${file.userName}`}>{file.userName}</a>
      </SummaryListItem>

      <SummaryListItem term="Date uploaded">
        <FormattedDate format="d MMMM yyyy HH:mm">{file.created}</FormattedDate>
      </SummaryListItem>

      <SummaryListItem term="Summary">
        <div className="dfe-white-space--pre-wrap">{file.summary}</div>
      </SummaryListItem>

      {canUpdateRelease && (
        <SummaryListItem
          term="Actions"
          actions={
            <>
              <Link
                className="govuk-!-margin-right-4"
                to={generatePath<ReleaseDataFileRouteParams>(
                  releaseAncillaryFileRoute.path,
                  {
                    publicationId,
                    releaseVersionId,
                    fileId: file.id,
                  },
                )}
              >
                Edit file
              </Link>
              <ModalConfirm
                title="Confirm deletion of file"
                triggerButton={
                  <ButtonText onClick={onDelete}>Delete file</ButtonText>
                }
                onExit={onCancelDelete}
                onCancel={onCancelDelete}
                onConfirm={onConfirmDelete}
              >
                <p>
                  This file will no longer be available for use in this release
                  ({file.filename})
                </p>
              </ModalConfirm>
            </>
          }
        />
      )}
    </SummaryList>
  );
}
