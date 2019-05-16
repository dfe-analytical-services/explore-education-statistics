import Link from '@frontend/components/Link';
import React from 'react';

interface Props {
  viewType?: string;
  topic?: string;
  link?: string;
}

const PrototypeDownloadDropdown = ({ link }: Props) => {
  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-one-third">
          <Link
            className="govuk-link govuk-!-margin-right-9 "
            to={link || '/prototypes/publication'}
          >
            View statistics and data
          </Link>
        </div>
        <div className="govuk-grid-column-one-third">
          <Link
            className="govuk-link govuk-!-margin-right-9 "
            to="/prototypes/table-tool"
          >
            Create your own tables online
          </Link>
        </div>
      </div>
    </>
  );
};

export default PrototypeDownloadDropdown;
