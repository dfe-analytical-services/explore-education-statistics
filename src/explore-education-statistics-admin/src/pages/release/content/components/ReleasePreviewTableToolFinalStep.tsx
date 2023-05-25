import Link from '@admin/components/Link';
import publicationService, {
  Publication,
  ExternalMethodology,
  Contact,
} from '@admin/services/publicationService';
import TableHeadersForm from '@common/modules/table-tool/components/TableHeadersForm';
import TimePeriodDataTable from '@common/modules/table-tool/components/TimePeriodDataTable';
import { FullTable } from '@common/modules/table-tool/types/fullTable';
import { TableHeadersConfig } from '@common/modules/table-tool/types/tableHeaders';
import DownloadTable from '@common/modules/table-tool/components/DownloadTable';
import TableToolInfo from '@common/modules/table-tool/components/TableToolInfo';
import tableBuilderService, {
  ReleaseTableDataQuery,
} from '@common/services/tableBuilderService';
import React, { ReactNode, useRef } from 'react';
import methodologyService, {
  MethodologyVersionSummary,
} from '@admin/services/methodologyService';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import LoadingSpinner from '@common/components/LoadingSpinner';

interface Model {
  methodologies: MethodologyVersionSummary[];
  externalMethodology?: ExternalMethodology | undefined;
  contact: Contact;
}

interface ReleasePreviewTableToolFinalStepProps {
  publication: Publication;
  query: ReleaseTableDataQuery;
  table: FullTable;
  tableHeaders: TableHeadersConfig;
  onReorderTableHeaders: (reorderedTableHeaders: TableHeadersConfig) => void;
}
const ReleasePreviewTableToolFinalStep = ({
  publication,
  query,
  table,
  tableHeaders,
  onReorderTableHeaders,
}: ReleasePreviewTableToolFinalStepProps) => {
  const dataTableRef = useRef<HTMLElement>(null);

  const { value: model, isLoading } = useAsyncHandledRetry<Model>(async () => {
    const [methodologies, externalMethodology, contact] = await Promise.all([
      methodologyService.listMethodologyVersions(publication.id),
      publicationService.getExternalMethodology(publication.id),
      publicationService.getContact(publication.id),
    ]);

    return { methodologies, externalMethodology, contact };
  }, [publication]);

  const getMethodologyLinks = () => {
    const links: ReactNode[] =
      model?.methodologies?.map(methodology => methodology.title) ?? [];

    if (model?.externalMethodology) {
      links.push(
        <Link
          key={model.externalMethodology.url}
          to={model.externalMethodology.url}
        >
          {model.externalMethodology.title}
        </Link>,
      );
    }
    return links;
  };

  return (
    <div className="govuk-!-margin-bottom-4">
      <TableHeadersForm
        initialValues={tableHeaders}
        onSubmit={nextTableHeaders => {
          onReorderTableHeaders(nextTableHeaders);
          if (dataTableRef.current) {
            // add a short delay so the reordering form is closed before it scrolls.
            setTimeout(() => {
              dataTableRef?.current?.scrollIntoView({
                behavior: 'smooth',
                block: 'start',
              });
            }, 200);
          }
        }}
      />

      {table && tableHeaders && (
        <TimePeriodDataTable
          ref={dataTableRef}
          footnotesClassName="govuk-!-width-two-thirds"
          fullTable={table}
          query={query}
          tableHeadersConfig={tableHeaders}
        />
      )}
      {publication && table && (
        <>
          <DownloadTable
            fullTable={table}
            fileName={`data-${publication.slug}`}
            onCsvDownload={() => tableBuilderService.getTableCsv(query)}
            tableRef={dataTableRef}
          />

          <LoadingSpinner loading={isLoading}>
            <TableToolInfo
              contactDetails={model?.contact}
              methodologyLinks={getMethodologyLinks()}
              releaseLink={<span>{publication.title}</span>}
            />
          </LoadingSpinner>
        </>
      )}
    </div>
  );
};

export default ReleasePreviewTableToolFinalStep;
