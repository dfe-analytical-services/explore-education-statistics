import ApprovalsTable from '@admin/pages/admin-dashboard/components/ApprovalsTable';
import { Release } from '@admin/services/releaseService';
import { MethodologyVersion } from '@admin/services/methodologyService';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React from 'react';

interface Props {
  isLoading: boolean;
  methodologyApprovals: MethodologyVersion[];
  releaseApprovals: Release[];
}

export default function ApprovalsTab({
  isLoading,
  methodologyApprovals,
  releaseApprovals,
}: Props) {
  return (
    <>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h2>Your approvals</h2>
          <p>
            Here you can view any releases or methodologies awaiting approval.
          </p>
        </div>
      </div>
      <LoadingSpinner hideText loading={isLoading} text="Loading approvals">
        <ApprovalsTable
          methodologyApprovals={methodologyApprovals}
          releaseApprovals={releaseApprovals}
        />
      </LoadingSpinner>
    </>
  );
}
