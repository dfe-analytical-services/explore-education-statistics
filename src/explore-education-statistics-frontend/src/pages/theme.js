import React, { Component } from 'react';
import DataList from '../components/datalist';
import Title from '../components/title';
import API from '../api';

class Theme extends Component {

    constructor(props) {
        super(props)
        this.state = {
            data: [],
            topics: []
        }
    }

    componentDidMount() {
        const { handle } = this.props.match.params;
        API.get(`theme/${handle}`)
            .then(json => this.setState({ data: json.data }))
            .catch(error => alert(error))
        API.get(`theme/${handle}/topics`)
            .then(json => this.setState({ topics: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const { data } = this.state;
        const { topics } = this.state;
        return (
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <Title label={data.title} />
                    <h2>What sort of stats are you looking for?</h2>
                    <DataList data={topics} linkIdentifier='topic' />
              </div>
            </div>
        );
    }
}

export default Theme;