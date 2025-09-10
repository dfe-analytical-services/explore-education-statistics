import render from '@common-test/render';
import ReleaseTableToolInfoWrapper from '@admin/pages/release/content/components/ReleaseTableToolInfoWrapper';
import _methodologyService from '@admin/services/methodologyService';
import _publicationService from '@admin/services/publicationService';
import {
  testContact,
  testPublication,
} from '@admin/pages/publication/__data__/testPublication';
import { screen, waitFor } from '@testing-library/react';
import React from 'react';

jest.mock('@admin/services/publicationService');
const publicationService = _publicationService as jest.Mocked<
  typeof _publicationService
>;
jest.mock('@admin/services/methodologyService');
const methodologyService = _methodologyService as jest.Mocked<
  typeof _methodologyService
>;

describe('ReleaseTableToolInfoWrapper', () => {
  test.only('renders successfully', async () => {
    publicationService.getContact.mockResolvedValue(testContact);
    publicationService.getExternalMethodology.mockResolvedValue({
      title: 'External methodology title',
      url: 'http://test.com',
    });
    methodologyService.listLatestMethodologyVersions.mockResolvedValue([
      {
        amendment: false,
        id: 'methodology-v1',
        methodologyId: 'methodology-1',
        owned: true,
        published: '2021-06-08T00:00:00',
        status: 'Approved',
        title: 'Methodology 1',
        permissions: {
          canUpdateMethodology: false,
          canDeleteMethodology: false,
          canMakeAmendmentOfMethodology: false,
          canApproveMethodology: false,
          canSubmitMethodologyForHigherReview: false,
          canMarkMethodologyAsDraft: false,
          canRemoveMethodologyLink: false,
        },
      },
    ]);

    render(
      <ReleaseTableToolInfoWrapper
        publication={testPublication}
        releaseType="OfficialStatistics"
      />,
    );

    expect(await screen.findByText('Related information')).toBeInTheDocument();
    expect(screen.getByText('Official statistics')).toBeInTheDocument();
    expect(screen.getByText('Publication 1')).toBeInTheDocument();
    expect(screen.getByText('Methodology: Methodology 1')).toBeInTheDocument();
    expect(
      screen.getByRole('link', { name: 'External methodology title' }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('heading', { name: 'Contact us' }),
    ).toBeInTheDocument();
    expect(screen.getByText('Team Smith')).toBeInTheDocument();
    expect(screen.getByText('john.smith@test.com')).toBeInTheDocument();
    expect(screen.getByText(/John Smith/)).toBeInTheDocument();
    expect(screen.getByText(/0777777777/)).toBeInTheDocument();
  });
});
