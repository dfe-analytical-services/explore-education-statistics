import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import React, { ReactNode } from 'react';
import releaseAncillaryFileService, {
  AncillaryFile,
} from '@admin/services/releaseAncillaryFileService';

interface Props {
  children?: ReactNode;
  ancillaryFile: AncillaryFile;
  releaseId: string;
}

const AncillaryFileDetailsTable = ({
  children,
  ancillaryFile,
  releaseId,
}: Props) => {
  return (
    <table>
      <tbody>
        <tr>
          <th scope="row" className="gov-!-width-one-third">
            Title
          </th>
          <td data-testid="Title">{ancillaryFile.title}</td>
        </tr>
        <tr>
          <th scope="row" className="gov-!-width-one-third">
            File
          </th>
          <td data-testid="File">
            <ButtonText
              onClick={() =>
                releaseAncillaryFileService.downloadFile(
                  releaseId,
                  ancillaryFile.id,
                  ancillaryFile.filename,
                )
              }
            >
              {ancillaryFile.filename}
            </ButtonText>
          </td>
        </tr>
        <tr>
          <th scope="row" className="gov-!-width-one-third">
            File size
          </th>
          <td data-testid="File size">{`${ancillaryFile.fileSize.size.toLocaleString()} ${
            ancillaryFile.fileSize.unit
          }`}</td>
        </tr>
        <tr>
          <th scope="row" className="gov-!-width-one-third">
            Uploaded by
          </th>
          <td data-testid="Uploaded by">
            <a href={`mailto:${ancillaryFile.userName}`}>
              {ancillaryFile.userName}
            </a>
          </td>
        </tr>
        <tr>
          <th scope="row" className="gov-!-width-one-third">
            Date uploaded
          </th>
          <td data-testid="Date uploaded">
            <FormattedDate format="d MMMM yyyy HH:mm">
              {ancillaryFile.created}
            </FormattedDate>
          </td>
        </tr>
        <tr>
          <th scope="row" className="gov-!-width-one-third">
            Summary
          </th>
          <td>
            <div className="dfe-white-space--pre-wrap" data-testid="Summary">
              {ancillaryFile.summary}
            </div>
          </td>
        </tr>
        {children && (
          <tr>
            <th scope="row">Actions</th>
            <td colSpan={2}>
              <div className="dfe-float--right">{children}</div>
            </td>
          </tr>
        )}
      </tbody>
    </table>
  );
};

export default AncillaryFileDetailsTable;
