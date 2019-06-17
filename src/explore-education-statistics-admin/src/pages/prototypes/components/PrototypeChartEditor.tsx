import React from 'react';

import classnames from 'classnames';

import styles from './PrototypeChartEditor.module.scss';

const PrototypeChartEditor = (props: {}) => {
  const types = ['line', 'horizontal', 'vertical', 'map'];

  return (
    <div className={styles.editor}>
      <p>Select chart type</p>

      <div className={styles.chartContainer}>
        {types.map(type => (
          <div key={type} className={classnames(styles.chart, styles[type])}>
            <div className={styles.title}>{type}</div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default PrototypeChartEditor;
