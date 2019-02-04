import React, { Component } from 'react';
import TopicList from './TopicList';

interface ThemeItem {
  id: string;
  slug: string;
  summary: string;
  title: string;
}

interface Props {
  items: ThemeItem[];
  linkIdentifier: string;
}

class ThemeList extends Component<Props> {
  public static defaultProps = {
    items: [],
  };

  public render() {
    const { items } = this.props;

    return (
      <>
        {items.length > 0 ? (
          <>
            {items.map(({ id, slug, title }) => (
              <div key={id}>
                <h2 className="govuk-heading-l">{title}</h2>
                <TopicList theme={slug} />
              </div>
            ))}
          </>
        ) : (
          <div className="govuk-inset-text">No data currently published.</div>
        )}
      </>
    );
  }
}

export default ThemeList;
