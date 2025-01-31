import Link from '@admin/components/Link';
import {
  releaseAncillaryFileRoute,
  ReleaseDataFileRouteParams,
} from '@admin/routes/releaseRoutes';
import releaseAncillaryFileService, {
  AncillaryFile,
} from '@admin/services/releaseAncillaryFileService';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React from 'react';
import { generatePath } from 'react-router-dom';

interface Props {
  canUpdateRelease?: boolean;
  file: AncillaryFile;
  publicationId: string;
  releaseId: string;
  onDelete?: () => void;
}

export default function AncillaryFileSummaryList({
  canUpdateRelease,
  file,
  publicationId,
  releaseId,
  onDelete,
}: Props) {
  return (
    <SummaryList>
      <SummaryListItem term="Title">{file.title}</SummaryListItem>

      <SummaryListItem term="File">
        <ButtonText
          onClick={() =>
            releaseAncillaryFileService.downloadFile(
              releaseId,
              file.id,
              file.filename,
            )
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
                    releaseId,
                    fileId: file.id,
                  },
                )}
              >
                Edit file
              </Link>
              <ButtonText onClick={onDelete}>Delete file</ButtonText>
            </>
          }
        />
      )}
    </SummaryList>
  );
}
