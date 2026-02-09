import { AuthContext, AuthContextState } from '@admin/contexts/AuthContext';
import { TestConfigContextProvider } from '@admin/contexts/ConfigContext';
import { ReleaseDataFileReplaceRouteParams } from '@admin/routes/releaseRoutes';
import { ImportStatusCode } from '@admin/services/releaseDataFileService';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { RouteComponentProps } from 'react-router';
import PendingDataReplacementSection from '../PendingDataReplacementSection';

jest.mock('@admin/services/releaseDataFileService', () => ({
  __esModule: true,
  default: {
    deleteDataFiles: jest.fn().mockResolvedValue(undefined),
  },
}));

const mockHistory: RouteComponentProps<ReleaseDataFileReplaceRouteParams>['history'] =
  {
    length: 1,
    location: { pathname: '/', search: '', state: undefined, hash: '' },
    push: jest.fn(),
    replace: jest.fn(),
    go: jest.fn(),
    goBack: jest.fn(),
    goForward: jest.fn(),
    block: jest.fn(),
    listen: jest.fn(),
    createHref: jest.fn(),
    action: 'PUSH',
  };

const defaultProps = {
  dataFileId: 'd4tad0e4-00fd-090a-ca30-0d00a0038ba0',
  replacementDataFile: {
    id: 'id',
    publicApiDataSetId: 'pup0ad0e4-00fd-090a-ca30-0d00a0038ba0',
    publicApiDataSetVersion: '2.0.0',
    title: 'Test Data File',
    fileName: 'test-file.csv',
    fileSize: { size: 12345, unit: 'bytes' },
    metaFileId: 'meta-id',
    metaFileName: 'test-meta-file.csv',
    userName: 'Test User',
    created: '2023-01-01T00:00:00Z',
    type: 'Data',
    rows: 100,
    status: 'COMPLETE' as ImportStatusCode,
    permissions: {
      canCancelReplacement: true,
      canReplaceData: true,
      canDelete: true,
      canCancelImport: true,
    },
  },
  publicApiDataSetId: 'pup0ad0e4-00fd-090a-ca30-0d00a0038ba0',
  replacementDataFileError: undefined,
  publicationId: 'p0pfd0e4-00fd-090a-ca30-0d00a0038ba0',
  releaseVersionId: 'rel2fd0e4-00fd-090a-ca30-0d00a0038ba0',
  history: mockHistory,
  fetchDataFile: jest.fn(),
};

const defaultPermissions = {
  isBauUser: true,
  canAccessSystem: true,
  canAccessPrereleasePages: true,
  canAccessAnalystPages: true,
  canAccessAllImports: true,
  canManageAllTaxonomy: true,
  isApprover: true,
};

const bau: AuthContextState['user'] = {
  id: 'user-1',
  name: 'Test User',
  permissions: defaultPermissions,
};

const analyst: AuthContextState['user'] = {
  ...bau,
  permissions: { ...defaultPermissions, isBauUser: false },
};

describe('PendingDataReplacementSection', () => {
  afterEach(() => {
    jest.clearAllMocks();
  });

  test('cancelling as an analyst when there is a public API linked to the data file is not possible', async () => {
    render(
      <TestConfigContextProvider>
        <AuthContext value={{ user: analyst }}>
          <PendingDataReplacementSection {...defaultProps} />,
        </AuthContext>
      </TestConfigContextProvider>,
    );

    expect(
      await screen.findByRole('button', { name: /cancel data replacement/i }),
    ).toBeInTheDocument();

    await userEvent.click(
      screen.getByRole('button', { name: /cancel data replacement/i }),
    );

    expect(
      await screen.findByText(
        /You do not have permission to cancel this data replacement. This is because it is linked to an API data set version which can only be modified by BAU users./i,
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByText('explore.statistics@education.gov.uk'),
    ).toHaveAttribute('href', 'mailto:explore.statistics@education.gov.uk');
    expect(
      screen.getByText(/Please contact the EES team for support/i),
    ).toBeInTheDocument();
    expect(
      screen.getByText(/explore.statistics@education.gov.uk/i),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: /confirm/i }),
    ).not.toBeInTheDocument();
  });

  test('cancelling as an bau when there is a public API linked to the data file is not possible', async () => {
    render(
      <TestConfigContextProvider>
        <AuthContext value={{ user: bau }}>
          <PendingDataReplacementSection {...defaultProps} />
        </AuthContext>
      </TestConfigContextProvider>,
    );

    expect(
      await screen.findByRole('button', { name: /cancel data replacement/i }),
    ).toBeInTheDocument();

    await userEvent.click(
      screen.getByRole('button', { name: /cancel data replacement/i }),
    );

    expect(
      screen.queryByText(/explore.statistics@education.gov.uk/i),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText(
        /You do not have permission to cancel this data replacement. This is because it is linked to an API data set version which can only be modified by BAU users./i,
      ),
    ).not.toBeInTheDocument();
    expect(
      screen.queryByText(
        /Are you sure you want to cancel this data replacement and remove the attached draft API version?/i,
      ),
    ).toBeInTheDocument();
    expect(
      screen.queryByRole('button', { name: /confirm/i }),
    ).toBeInTheDocument();
  });
});
