import PairedIndicatorsForm from '@admin/pages/release/data/components/PairedIndicatorsForm';
import ReorderIndicatorsList, {
  UpdateIndicatorsRequest,
} from '@admin/pages/release/data/components/ReorderIndicatorsList';
import adminTableBuilderService, {
  PairedIndicators,
} from '@admin/services/tableBuilderService';
import Button from '@common/components/Button';
import LoadingSpinner from '@common/components/LoadingSpinner';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import tableBuilderService, {
  Subject,
} from '@common/services/tableBuilderService';
import orderBy from 'lodash/orderBy';
import React, { useMemo, useState } from 'react';

type Mode = 'view' | 'reorder' | 'pair';

interface Props {
  releaseVersionId: string;
  subject: Subject;
  onBack: () => void;
}

/**
 * Shows a data set's indicator order and paired indicators read-only,
 * with separate screens for editing each of them.
 */
export default function ManageIndicators({
  releaseVersionId,
  subject,
  onBack,
}: Props) {
  const [mode, setMode] = useState<Mode>('view');

  const {
    value,
    isLoading,
    retry: reload,
  } = useAsyncHandledRetry(async () => {
    const [subjectMeta, { pairedIndicators }] = await Promise.all([
      tableBuilderService.getSubjectMeta(subject.id, releaseVersionId),
      adminTableBuilderService.getPairedIndicators(
        releaseVersionId,
        subject.id,
      ),
    ]);

    return { subjectMeta, pairedIndicators };
  }, [subject.id, releaseVersionId]);

  const indicatorGroups = useMemo(
    () => orderBy(Object.values(value?.subjectMeta.indicators ?? {}), 'order'),
    [value?.subjectMeta.indicators],
  );

  const indicatorLabels = useMemo(
    () =>
      Object.fromEntries(
        indicatorGroups
          .flatMap(group => group.options)
          .map(option => [option.value, option.label]),
      ),
    [indicatorGroups],
  );

  const handleSaveOrder = async (
    subjectId: string,
    request: UpdateIndicatorsRequest,
  ) => {
    await adminTableBuilderService.updateIndicators(
      releaseVersionId,
      subjectId,
      request,
    );
    reload();
    setMode('view');
  };

  const handleSavePairs = async (
    subjectId: string,
    request: PairedIndicators,
  ) => {
    await adminTableBuilderService.updatePairedIndicators(
      releaseVersionId,
      subjectId,
      request,
    );
    reload();
    setMode('view');
  };

  if (mode === 'reorder') {
    return (
      <div aria-live="polite">
        <ReorderIndicatorsList
          releaseVersionId={releaseVersionId}
          subject={subject}
          onCancel={() => setMode('view')}
          onSave={handleSaveOrder}
        />
      </div>
    );
  }

  if (mode === 'pair') {
    return (
      <div aria-live="polite">
        <PairedIndicatorsForm
          releaseVersionId={releaseVersionId}
          subject={subject}
          onCancel={() => setMode('view')}
          onSave={handleSavePairs}
        />
      </div>
    );
  }

  const pairedIndicators = value?.pairedIndicators ?? [];

  return (
    <div aria-live="polite">
      <h3>{`Manage indicators for ${subject.name}`}</h3>

      <LoadingSpinner loading={isLoading}>
        <section className="govuk-!-margin-bottom-8">
          <h4>Indicator order</h4>
          {indicatorGroups.length === 0 ? (
            <p>No indicators available.</p>
          ) : (
            <ol className="govuk-list govuk-list--number">
              {indicatorGroups.map(group => (
                <li key={group.id}>
                  {group.label}
                  <ol className="govuk-list govuk-list--number">
                    {group.options.map(option => (
                      <li key={option.value}>{option.label}</li>
                    ))}
                  </ol>
                </li>
              ))}
            </ol>
          )}
          <Button
            className="govuk-!-margin-bottom-0"
            disabled={indicatorGroups.length === 0}
            onClick={() => setMode('reorder')}
          >
            Reorder indicators
          </Button>
        </section>

        <section className="govuk-!-margin-bottom-8">
          <h4>Paired indicators</h4>
          <p className="govuk-hint">
            A child indicator is selected automatically with its parent
            indicator in the table tool, and cannot be selected on its own.
          </p>
          {pairedIndicators.length === 0 ? (
            <p>No indicators have been paired for this data set.</p>
          ) : (
            <table>
              <caption className="govuk-visually-hidden">
                {`Paired indicators for ${subject.name}`}
              </caption>
              <thead>
                <tr>
                  <th scope="col">Parent indicator</th>
                  <th scope="col">Child indicator</th>
                </tr>
              </thead>
              <tbody>
                {pairedIndicators.map(pair => (
                  <tr
                    key={`${pair.parentIndicatorId}-${pair.childIndicatorId}`}
                  >
                    <td>
                      {indicatorLabels[pair.parentIndicatorId] ??
                        pair.parentIndicatorId}
                    </td>
                    <td>
                      {indicatorLabels[pair.childIndicatorId] ??
                        pair.childIndicatorId}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
          <Button
            className="govuk-!-margin-bottom-0"
            onClick={() => setMode('pair')}
          >
            Edit paired indicators
          </Button>
        </section>
      </LoadingSpinner>

      <Button variant="secondary" onClick={onBack}>
        Back to data files
      </Button>
    </div>
  );
}
