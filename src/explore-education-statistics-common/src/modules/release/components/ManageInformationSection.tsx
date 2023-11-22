import React from 'react';

export default function ManagementInformationSection({
  showHeading = true,
}: {
  showHeading?: boolean;
}) {
  return (
    <>
      {showHeading && <h3>Management information</h3>}
      <p>
        Management information describes aggregate information collated and used
        in the normal course of business to inform operational delivery, policy
        development or the management of organisational performance. It is
        usually based on administrative data but can also be a product of survey
        data. The terms administrative data and management information are
        sometimes used interchangeably.
      </p>
    </>
  );
}
