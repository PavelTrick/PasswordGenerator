import { GenerateStatisticIteration } from "./generate-statistic-iteration.model";

export interface GenerateStatistic {
    id: number;
    passwordAmount: number;
    totalTime: number;
    statisticIterations: GenerateStatisticIteration[];
}