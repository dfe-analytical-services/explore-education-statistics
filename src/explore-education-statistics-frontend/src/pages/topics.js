import React, { Component } from 'react';
import DataList from '../components/datalist';
import Title from '../components/title';
import API from '../api';

class Topics extends Component {

    constructor(props) {
        super(props)
        this.state = {
            data: []
        }
    }

    componentDidMount() {
        API.get('topic')
            .then(json => this.setState({ data: json.data }))
            .catch(error => alert(error))
    }

    render() {
        const { data } = this.state;
        return (
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <Title label='Topics' />
                    <DataList data={data} linkIdentifier='topic' />
                </div>
            </div>
        );
    }
}

export default Topics;