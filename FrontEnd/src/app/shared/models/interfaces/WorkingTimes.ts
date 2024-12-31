export interface WorkingTimesViewModel {
    startTime: string,
    endTime: string,
    weekDay: WeekDayViewModel
}

export interface WeekDayViewModel {
      id: number,
      name: string,
      orderIndex? : number
}