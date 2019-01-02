import React, { Component } from 'react';
import DataList from '../components/datalist';
import Title from '../components/title';
import API from '../api';

class Topic extends Component {

    constructor(props) {
        super(props)
        this.state = {
            data: [],
            publications: []
        }
    }

    componentDidMount() {
        const { handle } = this.props.match.params;
        API.get(`topic/${handle}`)
            .then(json => this.setState({ data: json.data }))
            .catch(error => alert(error))
        API.get(`topic/${handle}/publications`)
        .then(json => this.setState({ publications: json.data }))
        .catch(error => alert(error))
    }

    render() {
        const { data } = this.state;
        const { publications } = this.state;
        return (
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <Title label={data.title} />
                    <h2 className="govuk-heading-l">Publications</h2>
                    <DataList data={publications} linkIdentifier="publication" />
                </div>
            </div>
        );
    }
}

export default Topic;