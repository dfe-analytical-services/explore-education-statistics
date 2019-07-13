import PrototypeEditableContent from '@admin/pages/prototypes/components/PrototypeEditableContent';
import PrototypeChartSample from '@admin/pages/prototypes/publication/components/PrototypeChartSample';
import Button from '@common/components/Button';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import PrototypeDataTilesHighlights from '@common/prototypes/publication/components/PrototypeDataTilesHighlights';
import PrototypeTableSample from '@common/prototypes/publication/components/PrototypeTableSample';
import React from 'react';

interface Props {
  editing?: boolean;
  reviewing?: boolean;
  sectionId?: string;
  chartTitle?: string;
  xAxisLabel?: string;
  yAxisLabel?: string;
  chartData?: object[];
  chartDataKeys: string[];
}

const PrototypeDataSample = ({
  editing,
  reviewing,
  sectionId,
  chartTitle,
  xAxisLabel,
  yAxisLabel,
  chartData,
  chartDataKeys,
}: Props) => {
  return (
    <>
      <Tabs id="data-sample-tabs">
        {sectionId === 'headlines' && (
          <TabsSection id={`${sectionId}SummaryData`} title="Summary">
            <PrototypeDataTilesHighlights />
            {editing && <Button>Add another key indicator</Button>}
            <PrototypeEditableContent
              editable={editing}
              reviewing={reviewing}
              content={`
            <ul className="govuk-list govuk-list--bullet">
              <li>pupils missed on average 8.2 school days</li>
              <li>
                overall and unauthorised absence rates up on previous year
              </li>
              <li>
                unauthorised rise due to higher rates of unauthorised holidays
              </li>
              <li>10% of pupils persistently absent during 2016/17</li>
            </ul>
          `}
            />
          </TabsSection>
        )}
        <TabsSection id={`${sectionId}TableData`} title="Data tables">
          <PrototypeTableSample caption={`Table showing ${chartTitle}`} />
        </TabsSection>
        <TabsSection id={`${sectionId}ChartData`} title="Charts">
          <h2 className="govuk-heading-s">{`Chart showing ${chartTitle}`}</h2>
          <PrototypeChartSample
            xAxisLabel={xAxisLabel}
            yAxisLabel={yAxisLabel}
            chartData={chartData}
            chartDataKeys={chartDataKeys}
          />
        </TabsSection>
      </Tabs>
    </>
  );
};

export default PrototypeDataSample;
